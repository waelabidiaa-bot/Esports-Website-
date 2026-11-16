using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class TeamApiService : BaseApiService
{
    private const string Endpoint = "api/team";

    public TeamApiService(ApiClient api) : base(api) { }

    public async Task<List<Team>> GetTeamsAsync()
    {
        var dtos = await Api.GetAllAsync<EsportPlatform.API.Data.DTOs.TeamDto.TeamDto>(Endpoint);
        var teams = new List<Team>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                var team = new Team
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Tag = dto.Tag,
                    GameId = dto.GameId,
                    Region = dto.Region,
                    Captain = dto.Captain,
                    RosterSize = dto.RosterSize,
                    MaxRoster = dto.MaxRoster,
                    Points = dto.Points,
                    Status = dto.Status,
                    CreatedAt = dto.CreatedAt
                };
                teams.Add(team);
            }
        }
        return teams;
    }

    public async Task<Team?> AddTeamAsync(Team team)
    {
        var dto = new EsportPlatform.API.Data.DTOs.TeamDto.TeamDto
        {
            Name = team.Name,
            Tag = team.Tag,
            GameId = team.GameId,
            Region = team.Region,
            Captain = team.Captain,
            RosterSize = team.RosterSize,
            MaxRoster = team.MaxRoster,
            Points = team.Points,
            Status = team.Status,
            CreatedAt = team.CreatedAt
        };

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.TeamDto.TeamDto>(
            HttpMethod.Post, Endpoint, dto);

        if (result is null) return null;

        return new Team
        {
            Id = result.Id,
            Name = result.Name,
            Tag = result.Tag,
            GameId = result.GameId,
            Region = result.Region,
            Captain = result.Captain,
            RosterSize = result.RosterSize,
            MaxRoster = result.MaxRoster,
            Points = result.Points,
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };
    }

    public async Task<Team?> RegisterTeamAsync(Team team, IEnumerable<Player> roster)
    {
        var teamDto = new
        {
            name = team.Name,
            tag = team.Tag,
            gameId = team.GameId,
            region = team.Region,
            captain = team.Captain,
            maxRoster = team.MaxRoster > 0 ? team.MaxRoster : 5
        };

        var rosterDtos = roster.Select(p => new
        {
            gamertag = p.Gamertag,
            fullName = string.IsNullOrWhiteSpace(p.FullName) ? p.Gamertag : p.FullName,
            email = p.Email ?? string.Empty,
            teamId = (int?)null,
            role = string.IsNullOrWhiteSpace(p.Role) ? "Roster" : p.Role,
            country = string.IsNullOrWhiteSpace(p.Country) ? team.Region : p.Country,
            status = string.IsNullOrWhiteSpace(p.Status) ? "Active" : p.Status
        }).ToList();

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.TeamDto.TeamDto>(
            HttpMethod.Post, $"{Endpoint}/register",
            new { team = teamDto, roster = rosterDtos });

        return result is null ? null : new Team
        {
            Id = result.Id,
            Name = result.Name,
            Tag = result.Tag,
            GameId = result.GameId,
            Region = result.Region,
            Captain = result.Captain,
            RosterSize = result.RosterSize,
            MaxRoster = result.MaxRoster,
            Points = result.Points,
            Status = result.Status,
            CreatedAt = result.CreatedAt
        };
    }

    public async Task<Player?> AddPlayerToTeamAsync(int teamId, Player player)
    {
        var dto = new
        {
            gamertag = player.Gamertag,
            fullName = string.IsNullOrWhiteSpace(player.FullName) ? player.Gamertag : player.FullName,
            email = player.Email ?? string.Empty,
            teamId = teamId,
            role = string.IsNullOrWhiteSpace(player.Role) ? "Roster" : player.Role,
            country = string.IsNullOrWhiteSpace(player.Country) ? "EU" : player.Country,
            status = string.IsNullOrWhiteSpace(player.Status) ? "Active" : player.Status
        };

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.PlayerDto.PlayerDto>(
            HttpMethod.Post, $"{Endpoint}/{teamId}/players", dto);

        return result is null ? null : new Player
        {
            Id = result.Id,
            Gamertag = result.Gamertag,
            FullName = result.FullName,
            Email = result.Email,
            TeamId = result.TeamId,
            Role = result.Role,
            Country = result.Country,
            Status = result.Status
        };
    }

    public async Task ApproveTeamAsync(int id)
    {
        await Api.SendAsync<object>(
            HttpMethod.Patch, $"{Endpoint}/{id}/status",
            new { status = "Approved" });
    }

    public async Task SuspendTeamAsync(int id)
    {
        await Api.SendAsync<object>(
            HttpMethod.Patch, $"{Endpoint}/{id}/status",
            new { status = "Suspended" });
    }
}