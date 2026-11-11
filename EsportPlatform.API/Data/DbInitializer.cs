using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data;

/// <summary>
/// Seeds the demo esports dataset once, so the Blazor client has data to display
/// immediately after the API is started against a fresh database.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var demoReady = await context.Games.AnyAsync(g => g.Name == "Valorant")
                        && await context.Teams.AnyAsync();

        if (demoReady)
        {
            await EnsureHighlightsAsync(context);
            return;
        }

        // Preserve a database that already contains real user data. The only case
        // we reset is an empty/partially seeded demo database with no teams or
        // tournaments (for example, stray records from manual API testing).
        if (await context.Teams.AnyAsync() || await context.Tournaments.AnyAsync())
        {
            return;
        }

        context.Games.RemoveRange(context.Games);
        await context.SaveChangesAsync();

        context.Games.AddRange(
            new Game { Id = 1, Name = "Valorant", Genre = "Tactical FPS", Publisher = "Riot Games", Platforms = "PC", Accent = "#ff4655", IsActive = true, Description = "5v5 character-based tactical shooter where precise gunplay meets unique agent abilities." },
            new Game { Id = 2, Name = "Counter-Strike 2", Genre = "Tactical FPS", Publisher = "Valve", Platforms = "PC", Accent = "#f5a623", IsActive = true, Description = "The legendary competitive shooter rebuilt on Source 2 with next-gen physics and lighting." },
            new Game { Id = 3, Name = "League of Legends", Genre = "MOBA", Publisher = "Riot Games", Platforms = "PC", Accent = "#00c8ff", IsActive = true, Description = "Two teams of five clash across the Rift in the world's biggest competitive MOBA." },
            new Game { Id = 4, Name = "Dota 2", Genre = "MOBA", Publisher = "Valve", Platforms = "PC", Accent = "#c23c2a", IsActive = true, Description = "Deep strategy, 120+ heroes and infinite drafts. The thinking player's MOBA." },
            new Game { Id = 5, Name = "Rocket League", Genre = "Sports", Publisher = "Psyonix", Platforms = "PC / Console", Accent = "#0ea5e9", IsActive = true, Description = "Soccer meets rocket-powered cars in a high-octane 3v3 aerial battleground." },
            new Game { Id = 6, Name = "Apex Legends", Genre = "Battle Royale", Publisher = "Respawn", Platforms = "PC / Console", Accent = "#da292a", IsActive = true, Description = "Legendary squad-based battle royale built around movement, teamwork and survival." },
            new Game { Id = 7, Name = "EA Sports FC 25", Genre = "Sports", Publisher = "EA Sports", Platforms = "PC / Console", Accent = "#22c55e", IsActive = true, Description = "The world's game with a competitive esports circuit and ultimate team drama." },
            new Game { Id = 8, Name = "Fortnite", Genre = "Battle Royale", Publisher = "Epic Games", Platforms = "PC / Console / Mobile", Accent = "#8b5cf6", IsActive = true, Description = "Build, battle and survive in the ever-evolving battle royale phenomenon." });

        context.Teams.AddRange(
            new Team { Id = 1, Name = "Phantom Vipers", Tag = "PVP", GameId = 1, Region = "EU", Captain = "Nova", RosterSize = 5, MaxRoster = 5, Points = 1480, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-8) },
            new Team { Id = 2, Name = "Neon Syndicate", Tag = "NSY", GameId = 1, Region = "NA", Captain = "Blaze", RosterSize = 4, MaxRoster = 5, Points = 1410, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-6) },
            new Team { Id = 3, Name = "Iron Wolves", Tag = "IRW", GameId = 2, Region = "EU", Captain = "Steel", RosterSize = 5, MaxRoster = 5, Points = 1520, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-11) },
            new Team { Id = 4, Name = "Ghost Protocol", Tag = "GHP", GameId = 2, Region = "NA", Captain = "Spectre", RosterSize = 4, MaxRoster = 5, Points = 1290, Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new Team { Id = 5, Name = "Void Runners", Tag = "VDR", GameId = 3, Region = "APAC", Captain = "Echo", RosterSize = 5, MaxRoster = 5, Points = 1360, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-4) },
            new Team { Id = 6, Name = "Nebula Esports", Tag = "NEB", GameId = 3, Region = "EU", Captain = "Orbit", RosterSize = 4, MaxRoster = 5, Points = 1330, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-5) },
            new Team { Id = 7, Name = "Crimson Order", Tag = "CRO", GameId = 4, Region = "EU", Captain = "Raven", RosterSize = 5, MaxRoster = 5, Points = 1445, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-9) },
            new Team { Id = 8, Name = "Storm Legion", Tag = "STL", GameId = 4, Region = "NA", Captain = "Tempest", RosterSize = 4, MaxRoster = 5, Points = 1275, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-3) },
            new Team { Id = 9, Name = "Solar Flare", Tag = "SLF", GameId = 5, Region = "NA", Captain = "Comet", RosterSize = 3, MaxRoster = 3, Points = 1180, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-7) },
            new Team { Id = 10, Name = "Nitro Boost", Tag = "NTB", GameId = 5, Region = "EU", Captain = "Turbo", RosterSize = 2, MaxRoster = 3, Points = 1120, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-2) },
            new Team { Id = 11, Name = "Apex Predators", Tag = "APX", GameId = 6, Region = "EU", Captain = "Fang", RosterSize = 2, MaxRoster = 3, Points = 1210, Status = "Approved", CreatedAt = DateTime.UtcNow.AddMonths(-5) },
            new Team { Id = 12, Name = "Skyfall", Tag = "SKY", GameId = 6, Region = "NA", Captain = "Wraith", RosterSize = 3, MaxRoster = 3, Points = 1090, Status = "Pending", CreatedAt = DateTime.UtcNow.AddDays(-1) });

        context.Players.AddRange(
            new Player { Id = 1, Gamertag = "Nova", FullName = "Alex Turner", Email = "nova@phantom.gg", TeamId = 1, Role = "Duelist", Country = "SE", Status = "Active" },
            new Player { Id = 2, Gamertag = "Frost", FullName = "Mila Kowalski", Email = "frost@phantom.gg", TeamId = 1, Role = "Controller", Country = "PL", Status = "Active" },
            new Player { Id = 3, Gamertag = "Krypt", FullName = "Daniel Reyes", Email = "krypt@phantom.gg", TeamId = 1, Role = "Sentinel", Country = "ES", Status = "Active" },
            new Player { Id = 4, Gamertag = "Blaze", FullName = "Jordan Cole", Email = "blaze@neon.gg", TeamId = 2, Role = "Duelist", Country = "US", Status = "Active" },
            new Player { Id = 5, Gamertag = "Vex", FullName = "Tyler Brooks", Email = "vex@neon.gg", TeamId = 2, Role = "Initiator", Country = "CA", Status = "Active" },
            new Player { Id = 6, Gamertag = "Steel", FullName = "Lukas Meyer", Email = "steel@wolves.gg", TeamId = 3, Role = "IGL", Country = "DE", Status = "Active" },
            new Player { Id = 7, Gamertag = "Bolt", FullName = "Sami Virtanen", Email = "bolt@wolves.gg", TeamId = 3, Role = "AWPer", Country = "FI", Status = "Active" },
            new Player { Id = 8, Gamertag = "Spectre", FullName = "Marcus Hale", Email = "spectre@ghost.gg", TeamId = 4, Role = "IGL", Country = "US", Status = "Active" },
            new Player { Id = 9, Gamertag = "Echo", FullName = "Hana Sato", Email = "echo@void.gg", TeamId = 5, Role = "Mid", Country = "JP", Status = "Active" },
            new Player { Id = 10, Gamertag = "Rift", FullName = "Min Jun Park", Email = "rift@void.gg", TeamId = 5, Role = "Jungle", Country = "KR", Status = "Active" },
            new Player { Id = 11, Gamertag = "Orbit", FullName = "Sofia Rossi", Email = "orbit@nebula.gg", TeamId = 6, Role = "Support", Country = "IT", Status = "Active" },
            new Player { Id = 12, Gamertag = "Raven", FullName = "Ivan Petrov", Email = "raven@crimson.gg", TeamId = 7, Role = "Carry", Country = "BG", Status = "Active" },
            new Player { Id = 13, Gamertag = "Tempest", FullName = "Owen Clarke", Email = "tempest@storm.gg", TeamId = 8, Role = "Offlane", Country = "UK", Status = "Active" },
            new Player { Id = 14, Gamertag = "Comet", FullName = "Noah Bennett", Email = "comet@solar.gg", TeamId = 9, Role = "Striker", Country = "US", Status = "Active" },
            new Player { Id = 15, Gamertag = "Turbo", FullName = "Elias Weber", Email = "turbo@nitro.gg", TeamId = 10, Role = "Goalkeeper", Country = "DE", Status = "Active" },
            new Player { Id = 16, Gamertag = "Fang", FullName = "Rina Kim", Email = "fang@apex.gg", TeamId = 11, Role = "Fragger", Country = "KR", Status = "Active" },
            new Player { Id = 17, Gamertag = "Wraith", FullName = "Diego Santos", Email = "wraith@sky.gg", TeamId = 12, Role = "Support", Country = "BR", Status = "Bench" });

        context.Tournaments.AddRange(
            new Tournament { Id = 1, Name = "Nexus Masters: Valorant", GameId = 1, Format = "Single Elimination", PrizePool = 50000, MaxTeams = 16, RegisteredTeams = 16, DebutDate = new DateTime(2026, 11, 1), FinDate = new DateTime(2026, 11, 15), Status = "Live", Description = "The flagship Valorant circuit event. Sixteen invited squads, one champion." },
            new Tournament { Id = 2, Name = "CS2 Pro League Season 8", GameId = 2, Format = "Double Elimination", PrizePool = 75000, MaxTeams = 16, RegisteredTeams = 12, DebutDate = new DateTime(2026, 11, 3), FinDate = new DateTime(2026, 11, 20), Status = "Live", Description = "Europe and North America collide in the longest-running CS2 league." },
            new Tournament { Id = 3, Name = "Rift Championship", GameId = 3, Format = "Groups + Playoffs", PrizePool = 40000, MaxTeams = 16, RegisteredTeams = 8, DebutDate = new DateTime(2026, 11, 12), FinDate = new DateTime(2026, 11, 28), Status = "Upcoming", Description = "Summoner's Rift supremacy. Group stage into a brutal single-elim bracket." },
            new Tournament { Id = 4, Name = "Turbo Cup", GameId = 5, Format = "Single Elimination", PrizePool = 15000, MaxTeams = 8, RegisteredTeams = 6, DebutDate = new DateTime(2026, 11, 9), FinDate = new DateTime(2026, 11, 11), Status = "Open", Description = "Fast, loud and aerial. Eight rocket-powered rosters fight for the cup." },
            new Tournament { Id = 5, Name = "Apex Survival Series", GameId = 6, Format = "Battle Royale", PrizePool = 25000, MaxTeams = 20, RegisteredTeams = 14, DebutDate = new DateTime(2026, 11, 18), FinDate = new DateTime(2026, 11, 25), Status = "Upcoming", Description = "Twenty squads drop in. Only one walks out with the crown." },
            new Tournament { Id = 6, Name = "Dota Dominion", GameId = 4, Format = "Single Elimination", PrizePool = 30000, MaxTeams = 16, RegisteredTeams = 10, DebutDate = new DateTime(2026, 12, 1), FinDate = new DateTime(2026, 12, 10), Status = "Open", Description = "Ancient warfare at its finest. Sixteen drafts, one Aegis." });

        context.Matches.AddRange(
            new Match { Id = 1, TournamentId = 1, GameId = 1, TeamAId = 1, TeamBId = 2, ScoreA = 2, ScoreB = 1, Status = "Live", MatchDate = new DateTime(2026, 11, 7, 18, 0, 0), Stage = "Grand Final", BestOf = "Bo5" },
            new Match { Id = 2, TournamentId = 2, GameId = 2, TeamAId = 3, TeamBId = 4, ScoreA = 1, ScoreB = 1, Status = "Live", MatchDate = new DateTime(2026, 11, 7, 19, 30, 0), Stage = "Group B", BestOf = "Bo3" },
            new Match { Id = 3, TournamentId = 6, GameId = 4, TeamAId = 7, TeamBId = 8, ScoreA = 0, ScoreB = 0, Status = "Upcoming", MatchDate = new DateTime(2026, 11, 8, 16, 0, 0), Stage = "Quarterfinal", BestOf = "Bo3" },
            new Match { Id = 4, TournamentId = 4, GameId = 5, TeamAId = 9, TeamBId = 10, ScoreA = 0, ScoreB = 0, Status = "Upcoming", MatchDate = new DateTime(2026, 11, 9, 20, 0, 0), Stage = "Semifinal", BestOf = "Bo5" },
            new Match { Id = 5, TournamentId = 3, GameId = 3, TeamAId = 5, TeamBId = 6, ScoreA = 2, ScoreB = 0, Status = "Completed", MatchDate = new DateTime(2026, 11, 5, 17, 0, 0), Stage = "Group A", BestOf = "Bo3" },
            new Match { Id = 6, TournamentId = 5, GameId = 6, TeamAId = 11, TeamBId = 12, ScoreA = 3, ScoreB = 2, Status = "Completed", MatchDate = new DateTime(2026, 11, 4, 21, 0, 0), Stage = "Survival Final", BestOf = "Bo5" },
            new Match { Id = 7, TournamentId = 1, GameId = 1, TeamAId = 2, TeamBId = 3, ScoreA = 13, ScoreB = 11, Status = "Completed", MatchDate = new DateTime(2026, 11, 2, 15, 0, 0), Stage = "Semifinal", BestOf = "Bo3" });

        context.Highlights.AddRange(GetDemoHighlights());

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Makes sure a database seeded before highlights existed still gets the demo
    /// highlight library, without touching any other user data.
    /// </summary>
    private static async Task EnsureHighlightsAsync(AppDbContext context)
    {
        if (await context.Highlights.AnyAsync())
        {
            return;
        }

        if (!await context.Games.AnyAsync())
        {
            return;
        }

        context.Highlights.AddRange(GetDemoHighlights());
        await context.SaveChangesAsync();
    }

    private static List<Highlight> GetDemoHighlights() =>
        new()
        {
            new Highlight { Id = 1, Title = "Nexus Masters — Grand Final Clutch", GameId = 1, Views = 45200, Duration = "12:48", MatchLabel = "Phantom Vipers vs Neon Syndicate", VideoUrl = "https://www.youtube.com/@NexusArena", PublishedAt = DateTime.UtcNow.AddHours(-1) },
            new Highlight { Id = 2, Title = "CS2 Pro League — Day 3 Best Plays", GameId = 2, Views = 28100, Duration = "08:12", MatchLabel = "Iron Wolves vs Ghost Protocol", VideoUrl = "https://www.youtube.com/@NexusArena", PublishedAt = DateTime.UtcNow.AddHours(-2) },
            new Highlight { Id = 3, Title = "Top 10 Clutches of the Week", GameId = 1, Views = 96400, Duration = "12:48", MatchLabel = "Weekly highlight reel", VideoUrl = "https://www.youtube.com/@NexusArena", PublishedAt = DateTime.UtcNow.AddDays(-1) },
            new Highlight { Id = 4, Title = "Rift Championship — Opening Show", GameId = 3, Views = 23800, Duration = "08:21", MatchLabel = "Opening ceremony", VideoUrl = "https://www.youtube.com/@NexusArena", PublishedAt = DateTime.UtcNow.AddDays(-2) },
            new Highlight { Id = 5, Title = "Turbo Cup Highlights", GameId = 5, Views = 19600, Duration = "06:54", MatchLabel = "Best aerial plays", VideoUrl = "https://www.youtube.com/@NexusArena", PublishedAt = DateTime.UtcNow.AddDays(-3) },
            new Highlight { Id = 6, Title = "Apex Survival Series — Finals Recap", GameId = 6, Views = 35100, Duration = "10:12", MatchLabel = "Apex Predators vs Skyfall", VideoUrl = "https://www.youtube.com/@NexusArena", PublishedAt = DateTime.UtcNow.AddDays(-4) }
        };
}
