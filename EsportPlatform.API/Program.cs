using System.Text.Json.Serialization;
using EsportPlatform.API.Data;
using EsportPlatform.API.services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("http://localhost:5146") // Blazor app origin
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Navigation properties are not loaded for list responses, but ignoring
        // cycles keeps the API safe if a caller requests an expanded graph.
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Dependency injection per request
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<TeamService>();
builder.Services.AddScoped<TournamentService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<PlayerService>();
builder.Services.AddScoped<HighlightService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Apply migrations and seed the demo dataset on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(db);
}

app.UseCors("AllowBlazorClient");

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    name = "Nexus Arena API",
    status = "ok",
    resources = new[]
    {
        "/api/game",
        "/api/team",
        "/api/tournament",
        "/api/match",
        "/api/player",
        "/api/highlight"
    }
}));

app.Run();
