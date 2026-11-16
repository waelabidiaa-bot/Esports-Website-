
using EsportPlatform.WEB.Components;
using EsportPlatform.WEB.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "..", ".aspnet-data-protection-keys-web");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("EsportPlatform.WEB");

// ==========================================
// RAZOR COMPONENTS
// ==========================================

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// ==========================================
// AUTHENTICATION AND AUTHORIZATION
// ==========================================

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>()
);

builder.Services.AddScoped<ProtectedSessionStorage>();

// ==========================================
// API SERVICES
// ==========================================

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<ApiDataService>();

builder.Services.AddScoped<GameApiService>();
builder.Services.AddScoped<TeamApiService>();
builder.Services.AddScoped<PlayerApiService>();
builder.Services.AddScoped<TournamentApiService>();
builder.Services.AddScoped<MatchApiService>();
builder.Services.AddScoped<HighlightApiService>();

// ==========================================
// HTTP CLIENT
// ==========================================

builder.Services.AddHttpClient(ApiHttpClient.Name, client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException(
            "Api:BaseUrl is not configured."
        );

    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
    client.Timeout = TimeSpan.FromSeconds(10);
    client.MaxResponseContentBufferSize = 8 * 1024 * 1024;
});

// ==========================================
// BUILD APPLICATION
// ==========================================

var app = builder.Build();

// ==========================================
// ERROR HANDLING
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true
);

// ==========================================
// HTTP MIDDLEWARE
// ==========================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAntiforgery();

// ==========================================
// ENDPOINTS
// ==========================================

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
