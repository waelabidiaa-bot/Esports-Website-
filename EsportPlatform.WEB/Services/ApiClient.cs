using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace EsportPlatform.WEB.Services;

public class ApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AuthService _authService;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public ApiClient(
        IHttpClientFactory httpClientFactory,
        AuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        using var request = CreateRequest(HttpMethod.Get, url);

        using var response = await GetHttpClient().SendAsync(request);

        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent ||
            response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    public async Task<T?> SendAsync<T>(
        HttpMethod method,
        string url,
        object? body = null)
    {
        using var request = CreateRequest(method, url);

        if (body is not null)
        {
            request.Content = JsonContent.Create(
                body,
                options: JsonOptions);
        }

        using var response = await GetHttpClient().SendAsync(request);

        response.EnsureSuccessStatusCode();

        if (response.StatusCode == HttpStatusCode.NoContent ||
            response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }

    private HttpRequestMessage CreateRequest(
        HttpMethod method,
        string url)
    {
        var request = new HttpRequestMessage(method, url);

        var token = _authService.Token;

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return request;
    }

    private HttpClient GetHttpClient()
    {
        return _httpClientFactory.CreateClient(ApiHttpClient.Name);
    }
}