using EsportPlatform.WEB.Components;
using EsportPlatform.WEB.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API HTTP client.
//
// Security: the base address comes from configuration ("Api:BaseUrl") so production
// can point at HTTPS, requests time out after 10 seconds, and response buffering is
// capped so a misbehaving API cannot exhaust server memory. A missing or invalid
// URL fails fast at startup instead of at first request.
//
// Performance: IHttpClientFactory pools and rotates the underlying handlers. The
// ApiDataService resolves a client per call, so handler rotation (and DNS refreshes)
// keep working even though the service is scoped to a long-lived circuit.
builder.Services.AddHttpClient(ApiHttpClient.Name, client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException(
            "Api:BaseUrl is not configured. Set it in appsettings.json (use HTTPS outside development).");

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(10);
    client.MaxResponseContentBufferSize = 8 * 1024 * 1024; // 8 MB safety cap
});

// ApiDataService — one instance per circuit, shared by every page in that circuit.
builder.Services.AddScoped<ApiDataService>();

var app = builder.Build();

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

// Static files
app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
