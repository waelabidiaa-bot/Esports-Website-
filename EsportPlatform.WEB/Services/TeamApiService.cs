using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class TeamApiService : BaseApiService
{
    private const string Endpoint = "api/team";

    public TeamApiService(ApiClient api) : base(api) { }

    public async Task<List<Team>> GetTeamsAsync()
    {
        var dtos = await Api.GetAsync<List<EsportPlatform.API.Data.DTOs.TeamDto.TeamDto>>(Endpoint);
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

    public async Task RegisterTeamAsync(Team team, IEnumerable<Player> roster)
    {
        var dto = new EsportPlatform.API.Data.DTOs.TeamDto.TeamDto
        {
            Name = team.Name,
            Tag = team.Tag,
            GameId = team.GameId,
            Region = team.Region,
            Captain = team.Captain,
            RosterSize = team.RosterSize,
            MaxRoster = team.MaxRoster
        };

        await Api.SendAsync<object>(
            HttpMethod.Post, $"{Endpoint}/register",
            new { team = dto, roster = roster.ToList() });
    }

    public async Task AddPlayerToTeamAsync(int teamId, Player player)
    {
        var sendDto = new
        {
            player = new
            {
                Gamertag = player.Gamertag,
                FullName = player.FullName,
                Email = player.Email,
                Role = player.Role,
                Country = player.Country,
                TeamId = player.TeamId
            },
            teamId
        };

        await Api.SendAsync<object>(
            HttpMethod.Post, $"{Endpoint}/{teamId}/players", sendDto);
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