using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Contracts;
using EsportPlatform.API.Models;
using EsportPlatform.API.services;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private static readonly string[] AllowedStatuses = { "Active", "Bench", "Inactive" };

    private readonly PlayerService _playerService;

    public PlayerController(PlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPlayers()
    {
        return Ok(await _playerService.GetAllPlayersAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayerById(int id)
    {
        var player = await _playerService.GetPlayerByIdAsync(id);
        if (player is null)
        {
            return NotFound("Player doesn't exist.");
        }

        return Ok(player);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePlayer([FromBody] Player player)
    {
        if (string.IsNullOrWhiteSpace(player.Gamertag))
        {
            return BadRequest("Gamertag is required.");
        }

        var created = await _playerService.CreatePlayerAsync(player);
        if (created is null)
        {
            return Conflict("That gamertag is already registered, or the roster is full.");
        }

        return CreatedAtAction(nameof(GetPlayerById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer([FromBody] Player player, int id)
    {
        var existing = await _playerService.GetPlayerByIdAsync(id);
        if (existing is null)
        {
            return NotFound("There is no player with that id.");
        }

        var updated = await _playerService.UpdatePlayerAsync(player, id);
        if (updated is null)
        {
            return Conflict("That gamertag is already registered, or the roster is full.");
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var deleted = await _playerService.DeletePlayerAsync(id);
        if (!deleted)
        {
            return NotFound("This player doesn't exist.");
        }

        return NoContent();
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] StatusRequest request)
    {
        if (!AllowedStatuses.Contains(request.Status))
        {
            return BadRequest("Status must be Active, Bench or Inactive.");
        }

        var player = await _playerService.SetStatusAsync(id, request.Status);
        if (player is null)
        {
            return NotFound("There is no player with that id.");
        }

        return Ok(player);
    }
}
