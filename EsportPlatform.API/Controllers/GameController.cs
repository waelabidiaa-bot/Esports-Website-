using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Data.DTOs.GameDto;
using EsportPlatform.API.Interfaces.services.interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    // Public: everyone can view games
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }

    // Public: everyone can view a specific game
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGameById(int id)
    {
        var game = await _gameService.GetGameByIdAsync(id);

        if (game is null)
            return NotFound("Game doesn't exist.");

        return Ok(game);
    }

    // Admin only: create a game
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameDto game)
    {
        var created = await _gameService.CreateGameAsync(game);

        if (created is null)
        {
            return Conflict(new
            {
                message = $"A game with the name '{game.Name}' already exists."
            });
        }

        return CreatedAtAction(
            nameof(GetGameById),
            new { id = created.Id },
            created
        );
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGame(
        [FromBody] UpdateGameDto game,
        int id)
    {
        var updated = await _gameService.UpdateGameAsync(id, game);

        if (updated is null)
        {
            return NotFound(new
            {
                message = $"Game with ID {id} was not found to update."
            });
        }

        return Ok(updated);
    }

    // Admin only: toggle game status
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleGame(int id)
    {
        var game = await _gameService.ToggleGameAsync(id);

        if (game is null)
            return NotFound("There is no game with that id.");

        return Ok(game);
    }
}