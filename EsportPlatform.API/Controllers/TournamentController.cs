using Microsoft.AspNetCore.Mvc;


using EsportPlatform.API.Data.DTOs.TournamentDto;
using EsportPlatform.API.Interfaces.services.interfaces;

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

    [HttpGet]
    public async Task<IActionResult> GetAllTournaments()
    {
        return Ok(await _tournamentService.GetAllTournamentsAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTournamentById(int id)
    {
        var tournament = await _tournamentService.GetTournamentByIdAsync(id);
        if (tournament is null)
        {
            return NotFound("Tournament doesn't exist.");
        }

        return Ok(tournament);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentDto tournament)
    {
        if (string.IsNullOrWhiteSpace(tournament.Name))
        {
            return BadRequest("Tournament name is required.");
        }

        var created = await _tournamentService.CreateTournamentAsync(tournament);
        if (created is null)
        {
            return Conflict("A tournament with that name already exists, or the game is invalid.");
        }

        return CreatedAtAction(nameof(GetTournamentById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTournament([FromBody] UpdateTournamentDto tournament, int id)
    {
        var existing = await _tournamentService.GetTournamentByIdAsync(id);
        if (existing is null)
        {
            return NotFound("There is no tournament with that id.");
        }

        var updated = await _tournamentService.UpdateTournamentAsync(tournament, id);
        if (updated is null)
        {
            return Conflict("A tournament with that name already exists, or the game is invalid.");
        }

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTournament(int id)
    {
        var deleted = await _tournamentService.DeleteTournamentAsync(id);
        if (!deleted)
        {
            return NotFound("This tournament doesn't exist.");
        }

        return NoContent();
    }
}
