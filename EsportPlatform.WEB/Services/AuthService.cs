using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using EsportPlatform.WEB.Models;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;

namespace EsportPlatform.WEB.Services;


public class AuthService
{
    private const string StorageKey = "egamesjam.auth.session";
    private const string LoginUrl = "api/auth/login";
    private const string RegisterUrl = "api/auth/register";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ProtectedSessionStorage _storage;

    private bool _restoreAttempted;

    public AuthService(IHttpClientFactory httpClientFactory, ProtectedSessionStorage storage)
    {
        _httpClientFactory = httpClientFactory;
        _storage = storage;
    }

    /// <summary>Raised whenever the signed-in user changes (login, register, logout, restore).</summary>
    public event Action? AuthStateChanged;

    public AuthSession? Session { get; private set; }

    public bool IsAuthenticated => Session is not null && ToUtc(Session.ExpiresAt) > DateTime.UtcNow;

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    public string? Token => Session?.Token;

    public string DisplayName =>
        Session is null
            ? string.Empty
            : string.IsNullOrWhiteSpace(Session.FullName) ? Session.UserName : Session.FullName;

    /// <summary>
    /// Loads a previously stored session once per circuit. Must run after the
    /// first render because it uses browser storage.
    /// </summary>
    public async Task RestoreAsync()
    {
        if (_restoreAttempted)
        {
            return;
        }

        _restoreAttempted = true;

        try
        {
            var stored = await _storage.GetAsync<AuthSession>(StorageKey);

            if (stored.Success && stored.Value is { } session && !string.IsNullOrWhiteSpace(session.Token))
            {
                if (ToUtc(session.ExpiresAt) > DateTime.UtcNow)
                {
                    PopulateRolesFromToken(session);

                    Session = session;
                    AuthStateChanged?.Invoke();
                    return;
                }

                Session = null;
                try { await _storage.DeleteAsync(StorageKey); } catch { }
                return;
            }

            try { await _storage.DeleteAsync(StorageKey); } catch { }
        }
        catch (InvalidOperationException)
        {
            // JavaScript interop is unavailable during prerendering; the next
            // interactive render can call RestoreAsync again.
            _restoreAttempted = false;
        }
        catch (JSDisconnectedException)
        {
            _restoreAttempted = false;
        }
        catch (Exception)
        {
            _restoreAttempted = false;
            try { await _storage.DeleteAsync(StorageKey); } catch { }
        }
    }

    public Task<AuthResult> LoginAsync(LoginRequest request) =>
        AuthenticateAsync(LoginUrl, request, "sign in");

    public Task<AuthResult> RegisterAsync(RegisterRequest request) =>
        AuthenticateAsync(RegisterUrl, request, "create your account");

    public async Task LogoutAsync()
    {
        Session = null;
        _restoreAttempted = true;

        try
        {
            await _storage.DeleteAsync(StorageKey);
        }
        catch
        {
            // Best-effort cleanup.
        }

        AuthStateChanged?.Invoke();
    }

    private async Task<AuthResult> AuthenticateAsync<TRequest>(
        string url,
        TRequest request,
        string action)
    {
        var http = _httpClientFactory.CreateClient(ApiHttpClient.Name);

        try
        {
            using var response = await http.PostAsJsonAsync(url, request, JsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                return AuthResult.Failure(await ReadErrorAsync(response));
            }

            var session = await response.Content.ReadFromJsonAsync<AuthSession>(JsonOptions);
            if (session is null || string.IsNullOrWhiteSpace(session.Token))
            {
                return AuthResult.Failure("The server returned an empty authentication response.");
            }

            PopulateRolesFromToken(session);
            Session = session;
            _restoreAttempted = true;

            try
            {
                await _storage.SetAsync(StorageKey, session);
            }
            catch (InvalidOperationException)
            {
                // Session storage is unavailable during prerendering.
            }
            catch (JSDisconnectedException)
            {
                // The browser circuit was disconnected before the session was stored.
            }
            catch
            {
                // Keep the in-memory session even if browser storage fails.
            }

            AuthStateChanged?.Invoke();
            return AuthResult.Success(session);
        }
        catch (Exception ex)
        {
            return AuthResult.Failure(ApiDataService.DescribeError(ex, action));
        }
    }

    private static void PopulateRolesFromToken(AuthSession session)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(session.Token))
            {
                return;
            }

            var jwtToken = handler.ReadJwtToken(session.Token);
            var roles = jwtToken.Claims
                .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (roles.Count > 0)
            {
                session.Roles = roles;
            }

            session.Role ??= session.Roles.FirstOrDefault() ??
                             jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

            if (string.IsNullOrWhiteSpace(session.UserName))
            {
                session.UserName = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name")?.Value ?? session.UserName;
            }

            if (string.IsNullOrWhiteSpace(session.FullName))
            {
                session.FullName = jwtToken.Claims.FirstOrDefault(c => c.Type == "fullName")?.Value ?? session.FullName;
            }
        }
        catch
        {
            // Best-effort role extraction; token may still be usable for API calls.
        }
    }

    /// <summary>
    /// Turns the API's three error shapes — a plain string, a ValidationProblemDetails
    /// object, or the Identity error array — into readable text.
    /// </summary>
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(content))
        {
            return response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                ? "Invalid username or password."
                : "The server couldn't process the request. Please try again.";
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            var root = document.RootElement;

            if (root.ValueKind == JsonValueKind.String)
            {
                return root.GetString() ?? content;
            }

            if (root.ValueKind == JsonValueKind.Array)
            {
                var messages = root.EnumerateArray()
                    .Select(ReadIdentityError)
                    .Where(message => !string.IsNullOrWhiteSpace(message));

                var joined = string.Join(" ", messages);
                if (!string.IsNullOrWhiteSpace(joined))
                {
                    return joined;
                }
            }

            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .SelectMany(property => property.Value.ValueKind == JsonValueKind.Array
                            ? property.Value.EnumerateArray().Select(item => item.GetString())
                            : new[] { property.Value.GetString() })
                        .Where(message => !string.IsNullOrWhiteSpace(message));

                    var joined = string.Join(" ", messages);
                    if (!string.IsNullOrWhiteSpace(joined))
                    {
                        return joined;
                    }
                }

                if (root.TryGetProperty("title", out var title) &&
                    title.GetString() is { Length: > 0 } titleText)
                {
                    return titleText;
                }
            }
        }
        catch (JsonException)
        {
            // Not JSON; fall through to the raw body.
        }

        return content;
    }

    private static string? ReadIdentityError(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty("description", out var description))
        {
            return description.GetString();
        }

        return null;
    }
}
