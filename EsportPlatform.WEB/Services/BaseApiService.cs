using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;


public abstract class BaseApiService
{
    /// <summary>
    /// Gets the API client for making HTTP requests.
    /// </summary>
    protected readonly ApiClient Api;

    protected BaseApiService(ApiClient api)
    {
        Api = api;
    }
}