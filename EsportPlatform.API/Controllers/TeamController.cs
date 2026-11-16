using Microsoft.AspNetCore.Mvc;
using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data.DTOs.TeamDto;
using EsportPlatform.API.Data.DTOs.PlayerDto;
using EsportPlatform.API.Interfaces.services.interfaces;
using Microsoft.AspNetCore.Authorization;

namespace EsportPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamController : ControllerBase
{
    private static readonly string[] AllowedStatuses =
    {
        "Approved", "Pending", "Suspended"
    };

    private readonly ITeamService _teamService;

    public TeamController(ITeamService teamService)
    {
        _teamService = teamService;
    }

   
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAllTeams(
        [FromQuery] PaginationQuery? pagination = null)
    {
        return Ok(await _teamService.GetAllTeamsPagedAsync(
            pagination ?? new PaginationQuery()));
    }

    // Public: everyone can view a specific team
    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTeamById(int id)
    {
        var team = await _teamService.GetTeamByIdAsync(id);

        if (team is null)
            return NotFound("Team doesn't exist.");

        return Ok(team);
    }

    
    [Authorize(Roles = "User,Admin,Player")]
    [HttpPost]
    public async Task<IActionResult> CreateTeam(
        [FromBody] CreateTeamDto team)
    {
        if (string.IsNullOrWhiteSpace(team.Name) ||
            string.IsNullOrWhiteSpace(team.Tag))
        {
            return BadRequest("Team name and tag are required.");
        }

        var created = await _teamService.CreateTeamAsync(team);

        if (created is null)
        {
            return Conflict(
                "A team with that name or tag already exists, or the game is invalid.");
        }

        return CreatedAtAction(
            nameof(GetTeamById),
            new { id = created.Id },
            created);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTeam(
        [FromBody] UpdateTeamDto team,
        int id)
    {
        var existing = await _teamService.GetTeamByIdAsync(id);

        if (existing is null)
            return NotFound("There is no team with that id.");

        var updated = await _teamService.UpdateTeamAsync(team, id);

        if (updated is null)
        {
            return Conflict(
                "A team with that name or tag already exists, or the game is invalid.");
        }

        return Ok(updated);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTeam(int id)
    {
        var deleted = await _teamService.DeleteTeamAsync(id);

        if (!deleted)
            return NotFound("This team doesn't exist.");

        return NoContent();
    }

    // Admin only: change team status
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> SetStatus(
        int id,
        [FromBody] StatusRequest request)
    {
        if (!AllowedStatuses.Contains(request.Status))
        {
            return BadRequest(
                "Status must be Approved, Pending or Suspended.");
        }

        var team = await _teamService.SetStatusAsync(id, request.Status);

        if (team is null)
            return NotFound("There is no team with that id.");

        return Ok(team);
    }

    
    [Authorize(Roles = "User,Admin,Player")]
    [HttpPost("register")]
    public async Task<IActionResult> RegisterTeam(
        [FromBody] TeamRegistrationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Team.Name) ||
            string.IsNullOrWhiteSpace(request.Team.Tag))
        {
            return BadRequest("Team name and tag are required.");
        }

        var team = await _teamService.RegisterTeamAsync(
            request.Team,
            request.Roster);

        if (team is null)
        {
            return Conflict(
                "A team with that name or tag already exists, or the game is invalid.");
        }

        return CreatedAtAction(
            nameof(GetTeamById),
            new { id = team.Id },
            team);
    }

    
    [Authorize(Roles = "User,Admin,Player")]
    [HttpPost("{id}/players")]
    public async Task<IActionResult> AddPlayer(
        int id,
        [FromBody] CreatePlayerDto player)
    {
        var existing = await _teamService.GetTeamByIdAsync(id);

        if (existing is null)
            return NotFound("There is no team with that id.");

        var created = await _teamService.AddPlayerToTeamAsync(id, player);

        if (created is null)
            return BadRequest("The roster is full.");

        return Ok(created);
    }
}