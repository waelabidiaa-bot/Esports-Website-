namespace EsportPlatform.Tests;

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Controllers;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class MatchControllerTest
{
    private (MatchController controller, AppDbContext context) GetControllerWithContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var service = new MatchService(context);
        var controller = new MatchController(service);

        return (controller, context);
    }

    private static async Task SeedTournamentWithTeamsAsync(AppDbContext context)
    {
        context.Games.Add(new Game { Id = 1, Name = "Valorant", Genre = "Tactical FPS" });
        context.Games.Add(new Game { Id = 2, Name = "Dota 2", Genre = "MOBA" });
        context.Tournaments.Add(new Tournament
        {
            Id = 1,
            Name = "Nexus Masters",
            GameId = 1,
            Status = "Live"
        });
        context.Teams.AddRange(
            new Team { Id = 1, Name = "Phantom Vipers", Tag = "PVP", GameId = 1 },
            new Team { Id = 2, Name = "Neon Syndicate", Tag = "NSY", GameId = 1 },
            new Team { Id = 3, Name = "Crimson Order", Tag = "CRO", GameId = 2 });
        await context.SaveChangesAsync();
    }

    // ==========================================
    // GET TESTS
    // ==========================================
    [Fact]
    public async Task GetAllMatches_ShouldReturnAllMatches()
    {
        var (controller, context) = GetControllerWithContext();
        context.Matches.AddRange(
            new Match { Id = 1, TournamentId = 1, GameId = 1, Stage = "Final" },
            new Match { Id = 2, TournamentId = 1, GameId = 1, Stage = "Semifinal" });
        await context.SaveChangesAsync();

        var result = await controller.GetAllMatches();

        var ok = Assert.IsType<OkObjectResult>(result);
        var matches = Assert.IsType<List<Match>>(ok.Value);
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public async Task GetMatchById_ShouldReturnMatch_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Matches.Add(new Match { Id = 5, TournamentId = 1, GameId = 1, Stage = "Group A" });
        await context.SaveChangesAsync();

        var result = await controller.GetMatchById(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        var match = Assert.IsType<Match>(ok.Value);
        Assert.Equal("Group A", match.Stage);
    }

    [Fact]
    public async Task GetMatchById_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.GetMatchById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // CREATE TESTS
    // ==========================================
    [Fact]
    public async Task CreateMatch_ShouldReturnCreated_AndInheritGameFromTournament()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedTournamentWithTeamsAsync(context);

        var result = await controller.CreateMatch(new Match
        {
            TournamentId = 1,
            TeamAId = 1,
            TeamBId = 2,
            Stage = "Grand Final",
            BestOf = "Bo5",
            Status = "Upcoming",
            MatchDate = DateTime.UtcNow.AddDays(1)
        });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var match = Assert.IsType<Match>(created.Value);
        Assert.Equal(1, match.GameId);
        Assert.Equal("Grand Final", match.Stage);
    }

    [Fact]
    public async Task CreateMatch_ShouldReturnBadRequest_WhenTournamentDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreateMatch(new Match { TournamentId = 999, TeamAId = 1, TeamBId = 2 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateMatch_ShouldReturnBadRequest_WhenBothSidesAreTheSameTeam()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedTournamentWithTeamsAsync(context);

        var result = await controller.CreateMatch(new Match { TournamentId = 1, TeamAId = 1, TeamBId = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateMatch_ShouldReturnBadRequest_WhenTeamPlaysAnotherGame()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedTournamentWithTeamsAsync(context);

        var result = await controller.CreateMatch(new Match { TournamentId = 1, TeamAId = 1, TeamBId = 3 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreateMatch_ShouldAllowUpcomingMatchWithoutTeams()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedTournamentWithTeamsAsync(context);

        var result = await controller.CreateMatch(new Match { TournamentId = 1, Stage = "TBD" });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var match = Assert.IsType<Match>(created.Value);
        Assert.Null(match.TeamAId);
        Assert.Null(match.TeamBId);
    }

    // ==========================================
    // UPDATE TESTS
    // ==========================================
    [Fact]
    public async Task UpdateMatch_ShouldReturnOk_WhenValid()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedTournamentWithTeamsAsync(context);
        context.Matches.Add(new Match { Id = 1, TournamentId = 1, GameId = 1, TeamAId = 1, TeamBId = 2 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateMatch(new Match
        {
            TournamentId = 1,
            TeamAId = 2,
            TeamBId = 1,
            ScoreA = 2,
            scoreB = 1,
            Stage = "Final",
            BestOf = "Bo5",
            Status = "Live",
            MatchDate = DateTime.UtcNow
        }, 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var match = Assert.IsType<Match>(ok.Value);
        Assert.Equal(2, match.ScoreA);
        Assert.Equal(1, match.scoreB);
        Assert.Equal("Final", match.Stage);
    }

    [Fact]
    public async Task UpdateMatch_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.UpdateMatch(new Match { TournamentId = 1 }, 999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateMatch_ShouldReturnBadRequest_WhenTeamsAreInvalid()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedTournamentWithTeamsAsync(context);
        context.Matches.Add(new Match { Id = 1, TournamentId = 1, GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateMatch(new Match { TournamentId = 1, TeamAId = 1, TeamBId = 1 }, 1);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==========================================
    // DELETE TESTS
    // ==========================================
    [Fact]
    public async Task DeleteMatch_ShouldReturnNoContent_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Matches.Add(new Match { Id = 1, TournamentId = 1, GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.DeleteMatch(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Matches);
    }

    [Fact]
    public async Task DeleteMatch_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.DeleteMatch(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // STATUS TESTS
    // ==========================================
    [Fact]
    public async Task SetStatus_ShouldReturnOk_WhenStatusIsValid()
    {
        var (controller, context) = GetControllerWithContext();
        context.Matches.Add(new Match { Id = 1, TournamentId = 1, GameId = 1, Status = "Upcoming" });
        await context.SaveChangesAsync();

        var result = await controller.SetStatus(1, new StatusRequest { Status = "Live" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var match = Assert.IsType<Match>(ok.Value);
        Assert.Equal("Live", match.Status);
    }

    [Fact]
    public async Task SetStatus_ShouldReturnBadRequest_WhenStatusIsInvalid()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.SetStatus(1, new StatusRequest { Status = "Paused" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SetStatus_ShouldReturnNotFound_WhenMatchDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.SetStatus(999, new StatusRequest { Status = "Live" });

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // SCORE TESTS
    // ==========================================
    [Fact]
    public async Task UpdateScore_ShouldReturnOk_WhenScoresAreValid()
    {
        var (controller, context) = GetControllerWithContext();
        context.Matches.Add(new Match { Id = 1, TournamentId = 1, GameId = 1, ScoreA = 0, scoreB = 0 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateScore(1, new ScoreRequest { ScoreA = 3, ScoreB = 2 });

        var ok = Assert.IsType<OkObjectResult>(result);
        var match = Assert.IsType<Match>(ok.Value);
        Assert.Equal(3, match.ScoreA);
        Assert.Equal(2, match.scoreB);
    }

    [Fact]
    public async Task UpdateScore_ShouldReturnBadRequest_WhenScoresAreNegative()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.UpdateScore(1, new ScoreRequest { ScoreA = -1, ScoreB = 0 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateScore_ShouldReturnNotFound_WhenMatchDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.UpdateScore(999, new ScoreRequest { ScoreA = 1, ScoreB = 0 });

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
