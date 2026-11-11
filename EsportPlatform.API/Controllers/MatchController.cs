using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Contracts;
using EsportPlatform.API.services;
using EsportPlatform.API.Data.DTOs.MatchDto;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchController : ControllerBase
{
    private static readonly string[] AllowedStatuses =
    {
        "Upcoming", "Live", "Completed"
    };

    private readonly MatchService _matchService;

    public MatchController(MatchService matchService)
    {
        _matchService = matchService;
    }










    [HttpGet]
    public async Task<IActionResult> GetAllMatches()
    {
        return Ok(await _matchService.GetAllMatchesAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMatchById(int id)
    {
        var match = await _matchService.GetMatchByIdAsync(id);

        if (match is null)
            return NotFound("Match doesn't exist.");

        return Ok(match);
    }
















    [HttpPost]
    public async Task<IActionResult> CreateMatch(
        [FromBody] CreateMatchDto match)
    {
        var created = await _matchService.CreateMatchAsync(match);

        if (created is null)
            return BadRequest(
                "Match details are invalid. Check the tournament and team selection.");

        return CreatedAtAction(
            nameof(GetMatchById),
            new { id = created.Id },
            created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMatch(
        int id,
        [FromBody] UpdateMatchDto match)
    {
        var updated = await _matchService.UpdateMatchAsync(match, id);

        if (updated is null)
            return BadRequest(
                "Match details are invalid. Check the tournament and team selection.");

        return Ok(updated);
    }














    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMatch(int id)
    {
        var deleted = await _matchService.DeleteMatchAsync(id);

        if (!deleted)
            return NotFound("This match doesn't exist.");

        return NoContent();
    }



















    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] StatusRequest request)
    {
        if (!AllowedStatuses.Contains(request.Status))
            return BadRequest(
                "Status must be Upcoming, Live or Completed.");

        var match = await _matchService.SetStatusAsync(id, request.Status);

        if (match is null)
            return NotFound("There is no match with that id.");

        return Ok(match);
    }












    

    [HttpPatch("{id}/score")]
    public async Task<IActionResult> UpdateScore(
        int id,
        [FromBody] ScoreRequest request)
    {
        if (request.ScoreA < 0 || request.ScoreB < 0)
            return BadRequest("Scores cannot be negative.");

        var match = await _matchService.UpdateScoreAsync(
            id, request.ScoreA, request.ScoreB);

        if (match is null)
            return NotFound("There is no match with that id.");

        return Ok(match);
    }
}