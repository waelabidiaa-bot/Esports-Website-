namespace EsportPlatform.Tests;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.services; 
using EsportPlatform.API.Controllers;
using Xunit;
using EsportPlatform.API.Data.DTOs.game;

public class GameControllerTest
{
    // ==========================================
    // REUSABLE HELPER (Modular Setup)
    // ==========================================
    private (GameController controller, AppDbContext context) GetControllerWithContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        var gameService = new GameService(context);
        var controller = new GameController(gameService);

        return (controller, context);
    }

    // ==========================================
    // 1. GET ALL GAMES TESTS
    // ==========================================
    [Fact]
    public async Task GetAllGames_ShouldReturnAllGames_WhenGamesExist()
    {
        // Arrange
        var (controller, context) = GetControllerWithContext();
        context.Games.Add(new Game { Id = 1, Name = "Counter-Strike 2", Genre = "FPS shooter", IsActive = true });
        context.Games.Add(new Game { Id = 2, Name = "League of Legends", Genre = "MOBA", IsActive = true });
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetAllGames();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var games = Assert.IsType<List<GameDto>>(okResult.Value);

        Assert.NotNull(games);
        Assert.Equal(2, games.Count);
        Assert.Contains(games, g => g.Name == "Counter-Strike 2");
    }

    [Fact]
    public async Task GetAllGames_ShouldReturnEmptyList_WhenNoGamesExist()
    {
        // Arrange
        var (controller, _) = GetControllerWithContext();

        // Act
        var result = await controller.GetAllGames();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var games = Assert.IsType<List<GameDto>>(okResult.Value); // Fixed: Should look for List<GameDto>

        Assert.NotNull(games);
        Assert.Empty(games);
    }

    // ==========================================
    // 2. GET GAME BY ID TESTS
    // ==========================================
    [Fact]
    public async Task GetGameById_ShouldReturnGame_WhenGameExists()
    {
        // Arrange
        var (controller, context) = GetControllerWithContext();
        var seededGame = new Game { Id = 1, Name = "Valorant", Genre = "Tactical Shooter", IsActive = true };
        context.Games.Add(seededGame);
        await context.SaveChangesAsync();

        // Act
        var result = await controller.GetGameById(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var game = Assert.IsType<GameDto>(okResult.Value); // Fixed: Controller returns GameDto

        Assert.NotNull(game);
        Assert.Equal("Valorant", game.Name);
    }

    [Fact]
    public async Task GetGameById_ShouldReturnNotFound_WhenGameDoesNotExist()
    {
        // Arrange
        var (controller, _) = GetControllerWithContext();

        // Act
        var result = await controller.GetGameById(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // ==========================================
    // 3. CREATE GAME TESTS
    // ==========================================
    [Fact]
    public async Task CreateGame_ShouldReturnCreated_WhenGameIsNew()
    {
        // Arrange
        var (controller, _) = GetControllerWithContext();
        var newGameDto = new CreateGameDto("Dota 2", "MOBA", "Strategy", "PC", true, Array.Empty<byte>());

        // Act
        var result = await controller.CreateGame(newGameDto);

        // Assert
        // Fixed: GameController uses StatusCode(201, new { message }) instead of CreatedAtAction
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreateGame_ShouldReturnConflict_WhenGameAlreadyExists()
    {
        // Arrange
        var (controller, context) = GetControllerWithContext();
        context.Games.Add(new Game { Id = 1, Name = "Dota 2", Genre = "MOBA", IsActive = true });
        await context.SaveChangesAsync();

        var duplicateGameDto = new CreateGameDto("Dota 2", "MOBA", "Strategy", "PC", true, Array.Empty<byte>());

        // Act
        var result = await controller.CreateGame(duplicateGameDto);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

        // ==========================================
    // 5. UPDATE GAME TESTS
    // ==========================================
    [Fact]
    public async Task UpdateGame_ShouldReturnOk_WhenGameExists()
    {
        // Arrange
        var (controller, context) = GetControllerWithContext();
        context.Games.Add(new Game { Id = 1, Name = "Old Name", Genre = "Strategy", IsActive = true });
        await context.SaveChangesAsync();

        // Fixed: Using object initializer syntax for GameDto
        var updatedGameDto = new GameDto 
        { 
            Id = 1, 
            Name = "Counter-Strike 2 Updated", 
            Genre = "FPS", 
            Description = "Tactical", 
            Platforms = "PC", 
            IsActive = true, 
            ImageData = Array.Empty<byte>() 
        };

        // Act
        var result = await controller.UpdateGame(updatedGameDto, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var game = Assert.IsType<GameDto>(okResult.Value);
        Assert.Equal("Counter-Strike 2 Updated", game.Name);
    }

    [Fact]
    public async Task UpdateGame_ShouldReturnNotFound_WhenGameDoesNotExist()
    {
        // Arrange
        var (controller, _) = GetControllerWithContext();
        
        // Fixed: Using object initializer syntax for GameDto
        var gameToUpdateDto = new GameDto 
        { 
            Id = 999, 
            Name = "Nonexistent", 
            Genre = "None", 
            Description = "None", 
            Platforms = "PC", 
            IsActive = true, 
            ImageData = Array.Empty<byte>() 
        };

        // Act
        var result = await controller.UpdateGame(gameToUpdateDto, 999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
    
    // ==========================================
    // 7. EXTENDED CATALOG UPDATE TESTS
    // ==========================================
    [Fact]
    public async Task UpdateGame_ShouldPersistExtendedCatalogFields()
    {
        // Arrange
        var (controller, context) = GetControllerWithContext();
        context.Games.Add(new Game { Id = 1, Name = "Apex", Genre = "BR", Description = "Old Desc", Platforms = "PC", IsActive = true });
        await context.SaveChangesAsync();

        // Fixed: Using object initializer syntax for GameDto
        var updatedGameDto = new GameDto 
        { 
            Id = 1, 
            Name = "Apex Legends", 
            Genre = "Battle Royale", 
            Description = "New Desc", 
            Platforms = "PC, PS5", 
            IsActive = true, 
            ImageData = Array.Empty<byte>() 
        };

        // Act
        var result = await controller.UpdateGame(updatedGameDto, 1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var game = Assert.IsType<GameDto>(okResult.Value);
        
        Assert.Equal("Apex Legends", game.Name);
        Assert.Equal("Battle Royale", game.Genre);
        Assert.Equal("New Desc", game.Description);
        Assert.Equal("PC, PS5", game.Platforms);
    }
