using System.Text.Json.Serialization;
using EsportPlatform.API.Data;

using Microsoft.EntityFrameworkCore;

using EsportPlatform.API.Interfaces.services.interfaces;
using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Data.Repositories;
using EsportPlatform.API.Repositories;
using EsportPlatform.API.services;
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

//Services
builder.Services.AddScoped<IGameService,GameService>();
builder.Services.AddScoped<ITeamService,TeamService>();
builder.Services.AddScoped<ITournamentService,TournamentService>();
builder.Services.AddScoped<IMatchService,MatchService>();
builder.Services.AddScoped<IPlayerService,PlayerService>();
builder.Services.AddScoped<IHighlightService,HighlightService>();


//Respos
//builder.Services.AddScoped<IRepository<T>,Repository<T>>();
builder.Services.AddScoped<IGameRepository ,GameRepository>();
builder.Services.AddScoped<IHighlightRepository ,HighlightRepository>();
builder.Services.AddScoped<IMatchRepository,MatchRepository>();
builder.Services.AddScoped<IPlayerRepository,PlayerRespository>();
builder.Services.AddScoped<ITeamRepository,TeamRepository>();

builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();


builder.Services.AddScoped(
    typeof(EsportPlatform.API.Interfaces.repositories.interfaces.IRepository<>), 
    typeof(EsportPlatform.API.Repositories.Repository<>)
);










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
    name = "EGame Jams",
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
