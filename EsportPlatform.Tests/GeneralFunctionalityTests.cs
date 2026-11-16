using System.Net;
using Microsoft.Playwright;
using Xunit;

namespace EsportPlatform.Tests;

/// <summary>
/// General end-to-end functionality tests using xUnit + Playwright.
/// 
/// Prerequisites:
/// - Install Playwright browsers once: `pwsh bin/Debug/net10.0/playwright.ps1 install` (or `playwright install`)
/// - Start the app(s) you want to test, then set base URL(s) via environment variables.
/// 
/// Environment variables:
/// - E2E_API_BASE_URL      (default: http://localhost:5040)   - EsportPlatform.API
/// - E2E_FRONTEND_BASE_URL (default: http://localhost:5146)   - EsportPlatform.WEB / frontend under test
/// 
/// Tests will fail with a clear message if the target service is not reachable.
/// </summary>
public class GeneralFunctionalityTests
{
    private static string ApiBaseUrl => (Environment.GetEnvironmentVariable("E2E_API_BASE_URL") ?? "http://localhost:5040").TrimEnd('/');
    private static string FrontendBaseUrl => (Environment.GetEnvironmentVariable("E2E_FRONTEND_BASE_URL") ?? "http://localhost:5146").TrimEnd('/');

    [Fact]
    [Trait("Category", "E2E")]
    public async Task Api_RootEndpoint_ShouldReturnOkStatusPayload()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        HttpResponseMessage response;
        try
        {
            response = await http.GetAsync($"{ApiBaseUrl}/");
        }
        catch (Exception ex)
        {
            throw new Xunit.Sdk.XunitException(
                $"Could not reach API at {ApiBaseUrl}. Start EsportPlatform.API and/or set E2E_API_BASE_URL. {ex.GetType().Name}: {ex.Message}");
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ok", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task Api_PublicEndpoints_ShouldBeReachable()
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var endpoints = new[] { "/api/game", "/api/team", "/api/tournament", "/api/match", "/api/player", "/api/highlight" };

        foreach (var endpoint in endpoints)
        {
            HttpResponseMessage response;
            try
            {
                response = await http.GetAsync($"{ApiBaseUrl}{endpoint}");
            }
            catch (Exception ex)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Could not reach API endpoint {ApiBaseUrl}{endpoint}. Start EsportPlatform.API and/or set E2E_API_BASE_URL. {ex.GetType().Name}: {ex.Message}");
            }

            Assert.True(response.StatusCode == HttpStatusCode.OK,
                $"Expected 200 OK from {endpoint} but got {(int)response.StatusCode} {response.StatusCode}.");
        }
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task Frontend_HomePage_ShouldLoadAndShowCoreNavigation()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        var page = await browser.NewPageAsync();

        try
        {
            var response = await page.GotoAsync(FrontendBaseUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 30000
            });

            Assert.NotNull(response);
            Assert.True(response!.Ok, $"Frontend home page returned non-OK status: {response.Status} at {FrontendBaseUrl}");

            // Basic smoke checks - content should include app branding/navigation text present in the platform UI.
            var content = await page.ContentAsync();
            Assert.False(string.IsNullOrWhiteSpace(content));

            var bodyText = await page.InnerTextAsync("body");
            Assert.Contains("Games", bodyText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Teams", bodyText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Tournaments", bodyText, StringComparison.OrdinalIgnoreCase);
        }
        catch (PlaywrightException ex)
        {
            throw new Xunit.Sdk.XunitException(
                $"Playwright could not test frontend at {FrontendBaseUrl}. Ensure browsers are installed (`playwright install`) and the frontend is running (or set E2E_FRONTEND_BASE_URL). {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            throw new Xunit.Sdk.XunitException(
                $"Timed out loading frontend at {FrontendBaseUrl}. Ensure the frontend is running and reachable. {ex.Message}");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    [Fact]
    [Trait("Category", "E2E")]
    public async Task Frontend_MainPublicPages_ShouldLoad()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await LaunchBrowserAsync(playwright);
        var page = await browser.NewPageAsync();

        var routes = new[] { "/", "/games", "/teams", "/tournaments", "/scores", "/players", "/highlights", "/login" };

        try
        {
            foreach (var route in routes)
            {
                var url = $"{FrontendBaseUrl}{route}";
                var response = await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });

                Assert.NotNull(response);
                Assert.True(response!.Ok, $"Expected OK for {url} but got {response.Status}.");

                var title = await page.TitleAsync();
                Assert.False(string.IsNullOrWhiteSpace(title));
            }
        }
        catch (PlaywrightException ex)
        {
            throw new Xunit.Sdk.XunitException(
                $"Playwright could not test frontend routes at {FrontendBaseUrl}. Ensure browsers are installed (`playwright install`) and the frontend is running (or set E2E_FRONTEND_BASE_URL). {ex.Message}");
        }
        catch (TimeoutException ex)
        {
            throw new Xunit.Sdk.XunitException(
                $"Timed out loading frontend routes at {FrontendBaseUrl}. Ensure the frontend is running and reachable. {ex.Message}");
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private static async Task<IBrowser> LaunchBrowserAsync(IPlaywright playwright)
    {
        try
        {
            return await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase)
                                            || ex.Message.Contains("browserType.launch", StringComparison.OrdinalIgnoreCase))
        {
            throw new Xunit.Sdk.XunitException(
                "Playwright browsers are not installed. Run `dotnet build EsportPlatform.Tests` then `pwsh bin/Debug/net10.0/playwright.ps1 install` (or `playwright install chromium`) and retry. " + ex.Message);
        }
    }
}
