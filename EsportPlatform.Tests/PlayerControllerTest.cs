namespace EsportPlatform.Tests;

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Controllers;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PlayerControllerTest
{
    private (PlayerController controller, AppDbContext context) GetControllerWithContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var service = new PlayerService(context);
        var controller = new PlayerController(service);

        return (controller, context);
    }

    // ==========================================
    // GET TESTS
    // ==========================================
    [Fact]
    public async Task GetAllPlayers_ShouldReturnAllPlayers()
    {
        var (controller, context) = GetControllerWithContext();
        context.Players.AddRange(
            new Player { Id = 1, Gamertag = "Nova" },
            new Player { Id = 2, Gamertag = "Frost" });
        await context.SaveChangesAsync();

        var result = await controller.GetAllPlayers();

        var ok = Assert.IsType<OkObjectResult>(result);
        var players = Assert.IsType<List<Player>>(ok.Value);
        Assert.Equal(2, players.Count);
    }

    [Fact]
    public async Task GetPlayerById_ShouldReturnPlayer_WhenItExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Players.Add(new Player { Id = 5, Gamertag = "Echo", Role = "Mid" });
        await context.SaveChangesAsync();

        var result = await controller.GetPlayerById(5);

        var ok = Assert.IsType<OkObjectResult>(result);
        var player = Assert.IsType<Player>(ok.Value);
        Assert.Equal("Echo", player.Gamertag);
    }

    [Fact]
    public async Task GetPlayerById_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.GetPlayerById(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // CREATE TESTS
    // ==========================================
    [Fact]
    public async Task CreatePlayer_ShouldReturnCreated_WhenGamertagIsNew()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreatePlayer(new Player
        {
            Gamertag = "Raven",
            FullName = "Ivan Petrov",
            Email = "raven@crimson.gg",
            Role = "Carry"
        });

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var player = Assert.IsType<Player>(created.Value);
        Assert.Equal("Raven", player.Gamertag);
    }

    [Fact]
    public async Task CreatePlayer_ShouldReturnConflict_WhenGamertagAlreadyExists()
    {
        var (controller, context) = GetControllerWithContext();
        context.Players.Add(new Player { Id = 1, Gamertag = "Raven" });
        await context.SaveChangesAsync();

        var result = await controller.CreatePlayer(new Player { Gamertag = "Raven" });

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task CreatePlayer_ShouldReturnBadRequest_WhenGamertagIsMissing()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.CreatePlayer(new Player { Gamertag = "   " });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreatePlayer_ShouldGrowRoster_WhenAssignedToTeam()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 1, Name = "Neon Syndicate", Tag = "NSY", GameId = 1, RosterSize = 2, MaxRoster = 5 });
        await context.SaveChangesAsync();

        var result = await controller.CreatePlayer(new Player { Gamertag = "Vex", TeamId = 1 });

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(3, context.Teams.Single().RosterSize);
    }

    [Fact]
    public async Task CreatePlayer_ShouldReturnConflict_WhenTeamRosterIsFull()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 1, Name = "Full Team", Tag = "FUL", GameId = 1, RosterSize = 3, MaxRoster = 3 });
        await context.SaveChangesAsync();

        var result = await controller.CreatePlayer(new Player { Gamertag = "Extra", TeamId = 1 });

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ==========================================
    // UPDATE TESTS
    // ==========================================
    [Fact]
    public async Task UpdatePlayer_ShouldReturnOk_WhenValid()
    {
        var (controller, context) = GetControllerWithContext();
        context.Players.Add(new Player { Id = 1, Gamertag = "Old Tag", Role = "Flex" });
        await context.SaveChangesAsync();

        var result = await controller.UpdatePlayer(new Player
        {
            Gamertag = "New Tag",
            FullName = "Updated Name",
            Email = "new@team.gg",
            Role = "IGL",
            Country = "DE",
            Status = "Active"
        }, 1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var player = Assert.IsType<Player>(ok.Value);
        Assert.Equal("New Tag", player.Gamertag);
        Assert.Equal("IGL", player.Role);
    }

    [Fact]
    public async Task UpdatePlayer_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.UpdatePlayer(new Player { Gamertag = "Ghost" }, 999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePlayer_ShouldReturnConflict_WhenGamertagBelongsToAnotherPlayer()
    {
        var (controller, context) = GetControllerWithContext();
        context.Players.AddRange(
            new Player { Id = 1, Gamertag = "Nova" },
            new Player { Id = 2, Gamertag = "Frost" });
        await context.SaveChangesAsync();

        var result = await controller.UpdatePlayer(new Player { Gamertag = "Nova" }, 2);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePlayer_ShouldMoveRosterCounts_WhenTeamChanges()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.AddRange(
            new Team { Id = 1, Name = "Old Team", Tag = "OLD", GameId = 1, RosterSize = 2, MaxRoster = 5 },
            new Team { Id = 2, Name = "New Team", Tag = "NEW", GameId = 1, RosterSize = 1, MaxRoster = 5 });
        context.Players.Add(new Player { Id = 1, Gamertag = "Mover", TeamId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.UpdatePlayer(new Player { Gamertag = "Mover", TeamId = 2 }, 1);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, context.Teams.Single(t => t.Id == 1).RosterSize);
        Assert.Equal(2, context.Teams.Single(t => t.Id == 2).RosterSize);
    }

    // ==========================================
    // DELETE TESTS
    // ==========================================
    [Fact]
    public async Task DeletePlayer_ShouldReturnNoContent_AndShrinkRoster()
    {
        var (controller, context) = GetControllerWithContext();
        context.Teams.Add(new Team { Id = 1, Name = "Neon Syndicate", Tag = "NSY", GameId = 1, RosterSize = 3, MaxRoster = 5 });
        context.Players.Add(new Player { Id = 1, Gamertag = "Leaver", TeamId = 1 });
        await context.SaveChangesAsync();

        var result = await controller.DeletePlayer(1);

        Assert.IsType<NoContentResult>(result);
        Assert.Empty(context.Players);
        Assert.Equal(2, context.Teams.Single().RosterSize);
    }

    [Fact]
    public async Task DeletePlayer_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.DeletePlayer(999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ==========================================
    // STATUS TESTS
    // ==========================================
    [Fact]
    public async Task SetStatus_ShouldReturnOk_WhenStatusIsValid()
    {
        var (controller, context) = GetControllerWithContext();
        context.Players.Add(new Player { Id = 1, Gamertag = "Nova", Status = "Active" });
        await context.SaveChangesAsync();

        var result = await controller.SetStatus(1, new StatusRequest { Status = "Bench" });

        var ok = Assert.IsType<OkObjectResult>(result);
        var player = Assert.IsType<Player>(ok.Value);
        Assert.Equal("Bench", player.Status);
    }

    [Fact]
    public async Task SetStatus_ShouldReturnBadRequest_WhenStatusIsInvalid()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.SetStatus(1, new StatusRequest { Status = "Retired" });

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SetStatus_ShouldReturnNotFound_WhenPlayerDoesNotExist()
    {
        var (controller, _) = GetControllerWithContext();

        var result = await controller.SetStatus(999, new StatusRequest { Status = "Active" });

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
