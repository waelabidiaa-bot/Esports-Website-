using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EsportPlatform.WEB.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace EsportPlatform.WEB.Services;

/// <summary>
/// Bridges the app's JWT/session-based AuthService into Blazor's authentication/authorization system.
/// </summary>
public sealed class JwtAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    private readonly AuthService _authService;

    public JwtAuthenticationStateProvider(AuthService authService)
    {
        _authService = authService;
        _authService.AuthStateChanged += OnAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Restore the persisted session before deciding whether the current
        // user is authenticated. Blazor's authorization components can ask for
        // the authentication state before any page/layout lifecycle method has
        // run (for example when the app shell is resumed after prerendering),
        // so waiting for a component's OnAfterRenderAsync to restore the session
        // would leave AuthorizeView stuck on its NotAuthorized branch.
        //
        // RestoreAsync is a no-op after the first attempt and safely ignores
        // the prerendering phase, where JavaScript interop is unavailable.
        await _authService.RestoreAsync();

        var principal = BuildPrincipal(_authService.Session);
        return new AuthenticationState(principal);
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private void OnAuthStateChanged()
    {
        NotifyAuthenticationStateChanged();
    }

    private static ClaimsPrincipal BuildPrincipal(AuthSession? session)
    {
        if (session is null || string.IsNullOrWhiteSpace(session.Token) || ToUtc(session.ExpiresAt) <= DateTime.UtcNow)
        {
            return Anonymous;
        }

        var claims = new List<Claim>();

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(session.Token))
            {
                var jwt = handler.ReadJwtToken(session.Token);
                // Preserve token claims (including role claims) and ensure basic identity claims exist.
                claims.AddRange(jwt.Claims);
            }
        }
        catch
        {
            // Fall back to session data if token parsing fails.
        }

        var userId = session.UserId;
        if (!string.IsNullOrWhiteSpace(userId) && claims.All(c => c.Type != ClaimTypes.NameIdentifier && c.Type != JwtRegisteredClaimNames.Sub))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        var name = !string.IsNullOrWhiteSpace(session.UserName) ? session.UserName : session.FullName;
        if (!string.IsNullOrWhiteSpace(name) && claims.All(c => c.Type != ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        // Ensure roles from session (API response / restored session) are present.
        foreach (var role in session.Roles.Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase)))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        if (!string.IsNullOrWhiteSpace(session.Role) &&
            !claims.Any(c => (c.Type == ClaimTypes.Role || c.Type == "role") && string.Equals(c.Value, session.Role, StringComparison.OrdinalIgnoreCase)))
        {
            claims.Add(new Claim(ClaimTypes.Role, session.Role));
        }

        // If we still couldn't establish identity details, don't authenticate.
        if (claims.Count == 0)
        {
            return Anonymous;
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt", nameType: ClaimTypes.Name, roleType: ClaimTypes.Role);
        return new ClaimsPrincipal(identity);
    }

    public void Dispose()
    {
        _authService.AuthStateChanged -= OnAuthStateChanged;
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
