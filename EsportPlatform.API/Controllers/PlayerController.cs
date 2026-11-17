using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data.DTOs.PlayerDto;
using EsportPlatform.API.Interfaces.services.interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayerController : ControllerBase
{
    private static readonly string[] AllowedStatuses =
    {
        "Active", "Bench", "Inactive"
    };

    private readonly IPlayerService _playerService;

    public PlayerController(IPlayerService playerService)
    {
        _playerService = playerService;
    }

    // Public: everyone can view players
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllPlayers(
        [FromQuery] PaginationQuery? pagination = null)
    {
        return Ok(await _playerService.GetAllPlayersPagedAsync(
            pagination ?? new PaginationQuery()));
    }

    // Admin only: roster memberships waiting for a decision
    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingPlayers()
    {
        return Ok(await _playerService.GetPendingPlayersAsync());
    }

    // Public: everyone can view a specific player
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPlayerById(int id)
    {
        var player = await _playerService.GetPlayerByIdAsync(id);

        if (player is null)
            return NotFound("Player doesn't exist.");

        return Ok(player);
    }

    // User, Player, or Admin: create a player profile
    [Authorize(Roles = "User,Admin,Player")]
    [HttpPost]
    public async Task<IActionResult> CreatePlayer(
        [FromBody] CreatePlayerDto player)
    {
        if (string.IsNullOrWhiteSpace(player.Gamertag))
            return BadRequest("Gamertag is required.");

        var created = await _playerService.CreatePlayerAsync(
            player,
            User.IsInRole("Admin"));

        if (created is null)
            return Conflict(
                "That gamertag is already registered, or the roster is full.");

        return CreatedAtAction(
            nameof(GetPlayerById),
            new { id = created.Id },
            created);
    }

    // Admin only: update a player
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePlayer(
        [FromBody] UpdatePlayerDto player,
        int id)
    {
        var existing = await _playerService.GetPlayerByIdAsync(id);

        if (existing is null)
            return NotFound("There is no player with that id.");

        var updated = await _playerService.UpdatePlayerAsync(
            player,
            id,
            User.IsInRole("Admin"));

        if (updated is null)
            return Conflict(
                "That gamertag is already registered, or the roster is full.");

        return Ok(updated);
    }

    // Admin only: delete a player
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePlayer(int id)
    {
        var deleted = await _playerService.DeletePlayerAsync(id);

        if (!deleted)
            return NotFound("This player doesn't exist.");

        return NoContent();
    }

    // Admin only: change player status
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] StatusRequest request)
    {
        if (!AllowedStatuses.Contains(request.Status))
            return BadRequest("Status must be Active, Bench or Inactive.");

        var player = await _playerService.SetStatusAsync(id, request.Status);

        if (player is null)
            return NotFound("There is no player with that id.");

        return Ok(player);
    }

    // Admin only: approve or reject a roster membership submitted by a user/player
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/approval")]
    public async Task<IActionResult> SetApproval(
        int id,
        [FromBody] ApprovalRequest request)
    {
        var player = await _playerService.SetApprovalAsync(id, request.Approved);

        if (player is null)
            return NotFound(
                "There is no player with that id, or the team roster is full.");

        return Ok(player);
    }
}