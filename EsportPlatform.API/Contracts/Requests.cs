using EsportPlatform.API.Data.DTOs.PlayerDto;
using EsportPlatform.API.Data.DTOs.TeamDto;
using EsportPlatform.API.Models;

namespace EsportPlatform.API.Contracts;

public class StatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class TeamRegistrationRequest
{
    public CreateTeamDto Team { get; set; } = default!;
    public List<CreatePlayerDto> Roster { get; set; } = new();
}
