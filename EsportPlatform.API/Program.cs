
using System.Text.Json.Serialization;
using System.Security.Claims;
using System.Text;

using EsportPlatform.API.Data;
using EsportPlatform.API.Data.Seeders;
using EsportPlatform.API.Identity;
using EsportPlatform.API.Interfaces.services.interfaces;
using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Data.Repositories;
using EsportPlatform.API.Repositories;
using EsportPlatform.API.services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// JWT CONFIGURATION
// ==========================================

var jwtSettings = builder.Configuration.GetSection("Jwt");

var secretKey = jwtSettings["SecretKey"]
    ?? throw new InvalidOperationException(
        "Jwt:SecretKey is not configured."
    );

if (Encoding.UTF8.GetByteCount(secretKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:SecretKey must be at least 32 characters long."
    );
}

builder.Services.Configure<JwtSettings>(jwtSettings);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            ),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

// ==========================================
// ASP.NET CORE IDENTITY
// ==========================================

builder.Services
    .AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// ==========================================
// DATABASE
// ==========================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// ==========================================
// CORS
// ==========================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
        policy.WithOrigins("http://localhost:5146")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ==========================================
// CONTROLLERS
// ==========================================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

// ==========================================
// SERVICES
// ==========================================

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ITournamentService, TournamentService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IHighlightService, HighlightService>();

// ==========================================
// REPOSITORIES
// ==========================================

builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IHighlightRepository, HighlightRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRespository>();
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITournamentRepository, TournamentRepository>();

builder.Services.AddScoped(
    typeof(EsportPlatform.API.Interfaces.repositories.interfaces.IRepository<>),
    typeof(EsportPlatform.API.Repositories.Repository<>)
);

// ==========================================
// BUILD APPLICATION
// ==========================================

var app = builder.Build();

// ==========================================
// DATABASE MIGRATIONS AND SEEDING
// ==========================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Apply EF Core migrations
    var db = services.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    // Seed demo dataset
    await DbInitializer.SeedAsync(db);

    // Create default roles
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    foreach (var roleName in new[] { "User", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var result = await roleManager.CreateAsync(
                new IdentityRole(roleName)
            );

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role {roleName}: " +
                    string.Join(", ", result.Errors.Select(e => e.Description))
                );
            }
        }
    }

    // Create initial admin account
    await AdminSeeder.SeedAdminAsync(
        services,
        builder.Configuration
    );
}

// ==========================================
// HTTP MIDDLEWARE
// ==========================================

app.UseCors("AllowBlazorClient");

app.UseAuthentication();

app.UseAuthorization();

// ==========================================
// ENDPOINTS
// ==========================================

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
        "/api/highlight",
        "/api/auth"
    }
}));

app.Run();
