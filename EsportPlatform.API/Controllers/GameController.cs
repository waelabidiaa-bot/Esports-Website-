using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;
using EsportPlatform.API.Data.DTOs.GameDto;
using EsportPlatform.API.Interfaces.services.interfaces;

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














    

    [HttpGet]
    public async Task<ActionResult<List<GameDto>>> GetAllGames()
    {
        var games = await _gameService.GetAllGamesAsync();
        return Ok(games);
    }


















    [HttpGet("{id}")]
    public async Task<ActionResult<GameDto>> GetGameById(int id)
    {
        var game = await _gameService.GetGameByIdAsync(id);
        if (game is null)
        {
            return NotFound("Game doesn't exist.");
        }

        return Ok(game);
    }














    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameDto game)
    {
        var created = await _gameService.CreateGameAsync(game);
        if (created is null)
        {
            return Conflict(new {message = $"A game with the name '{game.Name}' already exists."});
        }

        return CreatedAtAction(nameof(GetGameById),new {id=created.Id},created);
    }




















/*
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteGame(int id)
    {
        var deleted = await _gameService.DeleteGameAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = $"Game with ID {id} was not found to Delete." });
        }

        return NoContent();
    }*/

















    [HttpPut("{id}")]
    public async Task<ActionResult<GameDto>> UpdateGame([FromBody] UpdateGameDto game, int id)
    {
        var updated = await _gameService.UpdateGameAsync(id, game);
        if (updated is null)
        {
            return NotFound(new { message = $"Game with ID {id} was not found to update." });
        }

        return Ok(updated);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> ToggleGame(int id)
    {
        var game = await _gameService.ToggleGameAsync(id);
        if (game is null)
        {
            return NotFound("There is no game with that id.");
        }

        return Ok(game);
    }
}
