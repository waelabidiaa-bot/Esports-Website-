using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data.DTOs.TournamentDto;
using EsportPlatform.API.Interfaces.services.interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TournamentController : ControllerBase
{
    private readonly ITournamentService _tournamentService;

    public TournamentController(ITournamentService tournamentService)
    {
        _tournamentService = tournamentService;
    }

    // Public: everyone can view tournaments
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllTournaments(
        [FromQuery] PaginationQuery? pagination = null)
    {
        return Ok(await _tournamentService.GetAllTournamentsPagedAsync(
            pagination ?? new PaginationQuery()));
    }

    // Public: everyone can view a specific tournament
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTournamentById(int id)
    {
        var tournament = await _tournamentService.GetTournamentByIdAsync(id);

        if (tournament is null)
            return NotFound("Tournament doesn't exist.");

        return Ok(tournament);
    }

    // Admin only: create a tournament
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateTournament(
        [FromBody] CreateTournamentDto tournament)
    {
        if (string.IsNullOrWhiteSpace(tournament.Name))
            return BadRequest("Tournament name is required.");

        var created = await _tournamentService.CreateTournamentAsync(tournament);

        if (created is null)
        {
            return Conflict(
                "A tournament with that name already exists, or the game is invalid.");
        }

        return CreatedAtAction(
            nameof(GetTournamentById),
            new { id = created.Id },
            created);
    }

    // Admin only: update a tournament
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTournament(
        [FromBody] UpdateTournamentDto tournament,
        int id)
    {
        var existing = await _tournamentService.GetTournamentByIdAsync(id);

        if (existing is null)
            return NotFound("There is no tournament with that id.");

        var updated = await _tournamentService.UpdateTournamentAsync(tournament, id);

        if (updated is null)
        {
            return Conflict(
                "A tournament with that name already exists, or the game is invalid.");
        }

        return Ok(updated);
    }

    // Admin only: delete a tournament
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTournament(int id)
    {
        var deleted = await _tournamentService.DeleteTournamentAsync(id);

        if (!deleted)
            return NotFound("This tournament doesn't exist.");

        return NoContent();
    }
}