using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class TournamentApiService : BaseApiService
{
    private const string Endpoint = "api/tournament";

    public TournamentApiService(ApiClient api) : base(api) { }

    public async Task<List<Tournament>> GetTournamentsAsync()
    {
        var dtos = await Api.GetAsync<List<EsportPlatform.API.Data.DTOs.TournamentDto.TournamentDto>>(Endpoint);
        var tournaments = new List<Tournament>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                var tournament = new Tournament
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    GameId = dto.GameId,
                    Format = dto.Format,
                    PrizePool = dto.PrizePool,
                    MaxTeams = dto.MaxTeams,
                    RegisteredTeams = dto.RegisteredTeams,
                    Status = dto.Status,
                    Description = dto.Description
                };
                tournaments.Add(tournament);
            }
        }
        return tournaments;
    }

    public async Task<Tournament?> AddTournamentAsync(Tournament tournament)
    {
        var dto = new EsportPlatform.API.Data.DTOs.TournamentDto.TournamentDto
        {
            Name = tournament.Name,
            GameId = tournament.GameId,
            Format = tournament.Format,
            PrizePool = tournament.PrizePool,
            MaxTeams = tournament.MaxTeams,
            RegisteredTeams = tournament.RegisteredTeams,
            Status = tournament.Status,
            Description = tournament.Description
        };

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.TournamentDto.TournamentDto>(
            HttpMethod.Post, Endpoint, dto);

        if (result is null) return null;

        return new Tournament
        {
            Id = result.Id,
            Name = result.Name,
            GameId = result.GameId,
            Format = result.Format,
            PrizePool = result.PrizePool,
            MaxTeams = result.MaxTeams,
            RegisteredTeams = result.RegisteredTeams,
            Status = result.Status,
            Description = result.Description
        };
    }
}