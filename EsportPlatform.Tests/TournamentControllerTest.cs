namespace EsportPlatform.Tests;

using EsportPlatform.API.Controllers;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class TournamentControllerTest
{
    private (TournamentController controller, AppDbContext context) GetControllerWithContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var service = new TournamentService(context);
        var controller = new TournamentController(service);

        return (controller, context);
    }

    private static async Task SeedGameAsync(AppDbContext context, int id = 1)
    {
        context.Games.Add(new Game { Id = id, Name = $"Game {id}", Genre = "FPS" });
        await context.SaveChangesAsync();
    }

    // ==========================================
    // GET TESTS
    // ==========================================
    [Fact]
    public async Task GetAllTournaments_ShouldReturnAllTournaments()
    {
        var (controller, context) = GetControllerWithContext();
        context.Tournaments.AddRange(
            new Tournament { Id = 1, Name = "Nexus Masters", GameId = 1 },
            new Tournament { Id = 2, Name = "Turbo Cup", GameId = 2 });
        await context.SaveChangesAsync();

        var result = await controller.GetAllTournaments();

        var ok = Assert.IsType<OkObjectResult>(result);
        var tournaments = Assert.IsType<List<Tournament>>(ok.Value);
        Assert.Equal(2, tournaments.Count);
    }

    [Fact]
    public async Task GetAllTournaments_ShouldReturnEmptyList_WhenNoneExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.GetAllTournaments();

        var ok = Assert.IsType<OkObjectResult>(result);
        var tournaments = Assert.IsType<List<Tournament>>(ok.Value);
        Assert.Empty(tournaments);
    }

    [Fact]
    public async Task GetTournamentById_ShouldReturnTournament_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Tournaments.Add(new Tournament { Id = 3, Name = "Rift Championship", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.GetTournamentById(3);

        var ok = Assert.IsType<OkObjectResult>(result);
        var tournament = Assert.IsType<Tournament>(ok.Value);
        Assert.Equal("Rift Championship", tournament.Name);
    }

    [Fact]
    public async Task GetTournamentById_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.GetTournamentById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // CREATE TESTS
    // ==========================================
    [Fact]
    public async Task CreateTournament_ShouldReturnCreated_WhenValid()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);

        var result = await controller.CreateTournament(new Tournament
        {
            Name = "Winter Clash",
            GameId = 1,
            Format = "Double Elimination",
            PrizePool = 15000,
            MaxTeams = 8,
            DebutDate = DateTime.UtcNow.AddDays(5),
            FinDate = DateTime.UtcNow.AddDays(9)
        });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var tournament = Assert.IsType<Tournament>(created.Value);
        Assert.Equal("Winter Clash", tournament.Name);
        Assert.Equal(15000, tournament.PrizePool);
    }

    [Fact]
    public async Task CreateTournament_ShouldReturnConflict_WhenNameAlreadyExists()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Tournaments.Add(new Tournament { Id = 1, Name = "Winter Clash", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.CreateTournament(new Tournament { Name = "Winter Clash", GameId = 1 });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CreateTournament_ShouldReturnConflict_WhenGameDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreateTournament(new Tournament { Name = "Orphan Cup", GameId = 42 });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CreateTournament_ShouldReturnBadRequest_WhenNameIsMissing()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreateTournament(new Tournament { Name = "   ", GameId = 1 });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ==========================================
    // UPDATE TESTS
    // ==========================================
    [Fact]
    public async Task UpdateTournament_ShouldReturnOk_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Tournaments.Add(new Tournament { Id = 1, Name = "Old Cup", GameId = 1, PrizePool = 1000 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateTournament(new Tournament
        {
            Name = "New Cup",
            GameId = 1,
            Format = "Groups + Playoffs",
            PrizePool = 5000,
            MaxTeams = 16,
            RegisteredTeams = 4,
            Status = "Open",
            Description = "Updated"
        }, 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var tournament = Assert.IsType<Tournament>(ok.Value);
        Assert.Equal("New Cup", tournament.Name);
        Assert.Equal(5000, tournament.PrizePool);
        Assert.Equal("Open", tournament.Status);
    }

    [Fact]
    public async Task UpdateTournament_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.UpdateTournament(new Tournament { Name = "Ghost Cup", GameId = 1 }, 999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateTournament_ShouldReturnConflict_WhenNameBelongsToAnotherTournament()
    {
        var (controller, context) = GetControllerWithContext();
        await SeedGameAsync(context);
        context.Tournaments.AddRange(
            new Tournament { Id = 1, Name = "Cup One", GameId = 1 },
            new Tournament { Id = 2, Name = "Cup Two", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.UpdateTournament(new Tournament { Name = "Cup One", GameId = 1 }, 2);

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ==========================================
    // DELETE TESTS
    // ==========================================
    [Fact]
    public async Task DeleteTournament_ShouldReturnNoContent_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Tournaments.Add(new Tournament { Id = 1, Name = "Delete Cup", GameId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.DeleteTournament(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Tournaments);
    }

    [Fact]
    public async Task DeleteTournament_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.DeleteTournament(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
