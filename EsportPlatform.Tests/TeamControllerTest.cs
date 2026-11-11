namespace EsportPlatform.Tests;

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Controllers;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TeamControllerTest
{
    private (TeamController controller, AppDbContext context) GetControllerWithContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var service = new TeamService(context);
        var controller = new TeamController(service);

        return (controller, context);
    }

    private static async Task<Game> SeedGameAsync(AppDbContext context, int id = 1)
    {
        var game = new Game { Id = id, Name = $"Game {id}", Genre = "FPS" };
        context.Games.Add(game);
        await context.SaveChangesAsync();
        return game;
    }

    // ==========================================
    // GET TESTS
    // ==========================================
    [Fact]
    public async Task GetAllTeams_ShouldReturnAllTeams()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.AddRange(
            new Team { Id = 1, Name = "Phantom Vipers", Tag = "PVP", GameId = 1 },
            new Team { Id = 2, Name = "Neon Syndicate", Tag = "NSY", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.GetAllTeams();

        var ok = Assert.IsType<OkObjectResult>(result);
        var teams = Assert.IsType<List<Team>>(ok.Value);
        Assert.Equal(2, teams.Count);
    }

    [Fact]
    public async Task GetTeamById_ShouldReturnTeam_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 7, Name = "Iron Wolves", Tag = "IRW", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.GetTeamById(7);

        var ok = Assert.IsType<OkObjectResult>(result);
        var team = Assert.IsType<Team>(ok.Value);
        Assert.Equal("Iron Wolves", team.Name);
    }

    [Fact]
    public async Task GetTeamById_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.GetTeamById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // CREATE TESTS
    // ==========================================
    [Fact]
    public async Task CreateTeam_ShouldReturnCreated_WhenTeamIsValid()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);

        var result = await controller.CreateTeam(new Team
        {
            Name = "Solar Flare",
            Tag = "SLF",
            GameId = 1,
            Region = "NA"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var team = Assert.IsType<Team>(created.Value);
        Assert.Equal("Solar Flare", team.Name);
        Assert.Equal("Approved", team.Status);
    }

    [Fact]
    public async Task CreateTeam_ShouldReturnConflict_WhenNameOrTagAlreadyExists()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Teams.Add(new Team { Id = 1, Name = "Iron Wolves", Tag = "IRW", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.CreateTeam(new Team
        {
            Name = "Different Name",
            Tag = "IRW",
            GameId = 1
        });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CreateTeam_ShouldReturnConflict_WhenGameDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreateTeam(new Team
        {
            Name = "Orphan Team",
            Tag = "ORP",
            GameId = 42
        });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CreateTeam_ShouldReturnBadRequest_WhenNameOrTagIsMissing()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreateTeam(new Team { Name = "", Tag = "ABC", GameId = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==========================================
    // UPDATE TESTS
    // ==========================================
    [Fact]
    public async Task UpdateTeam_ShouldReturnOk_WhenTeamExists()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Teams.Add(new Team { Id = 1, Name = "Old Name", Tag = "OLD", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateTeam(new Team
        {
            Name = "New Name",
            Tag = "NEW",
            GameId = 1,
            Region = "EU",
            Captain = "Nova",
            RosterSize = 5,
            MaxRoster = 5,
            Points = 1200,
            Status = "Approved"
        }, 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var team = Assert.IsType<Team>(ok.Value);
        Assert.Equal("New Name", team.Name);
        Assert.Equal("Nova", team.Captain);
    }

    [Fact]
    public async Task UpdateTeam_ShouldReturnNotFound_WhenTeamDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.UpdateTeam(new Team { Name = "Ghost", Tag = "GHO", GameId = 1 }, 999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTeam_ShouldReturnConflict_WhenAnotherTeamUsesTheTag()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Teams.AddRange(
            new Team { Id = 1, Name = "Iron Wolves", Tag = "IRW", GameId = 1 },
            new Team { Id = 2, Name = "Neon Syndicate", Tag = "NSY", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateTeam(new Team
        {
            Name = "Neon Syndicate",
            Tag = "IRW",
            GameId = 1
        }, 2);

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ==========================================
    // DELETE TESTS
    // ==========================================
    [Fact]
    public async Task DeleteTeam_ShouldReturnNoContent_WhenTeamExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 1, Name = "Delete Me", Tag = "DEL", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.DeleteTeam(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Teams);
    }

    [Fact]
    public async Task DeleteTeam_ShouldReturnNotFound_WhenTeamDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.DeleteTeam(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // STATUS TESTS
    // ==========================================
    [Fact]
    public async Task SetStatus_ShouldReturnOk_WhenStatusIsValid()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 1, Name = "Pending Team", Tag = "PND", GameId = 1, Status = "Pending" });
        await context.SaveChangesAsync();

        var result = await controller.SetStatus(1, new StatusRequest { Status = "Approved" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var team = Assert.IsType<Team>(ok.Value);
        Assert.Equal("Approved", team.Status);
    }

    [Fact]
    public async Task SetStatus_ShouldReturnBadRequest_WhenStatusIsInvalid()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.SetStatus(1, new StatusRequest { Status = "Banned" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SetStatus_ShouldReturnNotFound_WhenTeamDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.SetStatus(999, new StatusRequest { Status = "Approved" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // REGISTRATION TESTS
    // ==========================================
    [Fact]
    public async Task RegisterTeam_ShouldCreatePendingTeamWithRoster_AndReserveTournamentSlot()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Tournaments.Add(new Tournament
        {
            Id = 1,
            Name = "Open Cup",
            GameId = 1,
            Status = "Open",
            MaxTeams = 8,
            RegisteredTeams = 2,
            DebutDate = DateTime.UtcNow.AddDays(1)
        });
        await context.SaveChangesAsync();

        var request = new TeamRegistrationRequest
        {
            Team = new Team { Name = "New Blood", Tag = "NBL", GameId = 1, Region = "EU" },
            Roster = new List<Player>
            {
                new() { Gamertag = "Alpha", Role = "Captain" },
                new() { Gamertag = "Bravo", Role = "Roster" }
            }
        };

        var result = await controller.RegisterTeam(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var team = Assert.IsType<Team>(created.Value);
        Assert.Equal("Pending", team.Status);
        Assert.Equal(2, team.RosterSize);
        Assert.Equal(2, context.Players.Count());
        Assert.All(context.Players, p => Assert.Equal(team.Id, p.TeamId));
        Assert.Equal(3, context.Tournaments.Single().RegisteredTeams);
    }

    [Fact]
    public async Task RegisterTeam_ShouldReturnConflict_WhenTeamNameExists()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Teams.Add(new Team { Id = 1, Name = "New Blood", Tag = "NBL", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.RegisterTeam(new TeamRegistrationRequest
        {
            Team = new Team { Name = "New Blood", Tag = "NEW", GameId = 1 }
        });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RegisterTeam_ShouldReturnBadRequest_WhenNameOrTagIsMissing()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.RegisterTeam(new TeamRegistrationRequest
        {
            Team = new Team { Name = "No Tag", Tag = "", GameId = 1 }
        });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddPlayer_ShouldReturnOk_AndGrowRoster()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 1, Name = "Neon Syndicate", Tag = "NSY", GameId = 1, RosterSize = 2, MaxRoster = 5 });
        await context.SaveChangesAsync();

        var result = await controller.AddPlayer(1, new Player { Gamertag = "Vex", Role = "Initiator" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var player = Assert.IsType<Player>(ok.Value);
        Assert.Equal(1, player.TeamId);
        Assert.Equal(3, context.Teams.Single().RosterSize);
    }

    [Fact]
    public async Task AddPlayer_ShouldReturnBadRequest_WhenRosterIsFull()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 1, Name = "Full Team", Tag = "FUL", GameId = 1, RosterSize = 3, MaxRoster = 3 });
        await context.SaveChangesAsync();

        var result = await controller.AddPlayer(1, new Player { Gamertag = "Extra" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AddPlayer_ShouldReturnNotFound_WhenTeamDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.AddPlayer(999, new Player { Gamertag = "Lost" });

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
