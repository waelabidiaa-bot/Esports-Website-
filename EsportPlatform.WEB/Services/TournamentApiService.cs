using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class TournamentApiService : BaseApiService
{
    private const string Endpoint = "api/tournament";

    public TournamentApiService(ApiClient api) : base(api) { }

    public async Task<List<Tournament>> GetTournamentsAsync()
    {
        var dtos = await Api.GetAllAsync<EsportPlatform.API.Data.DTOs.TournamentDto.TournamentDto>(Endpoint);
        var tournaments = new List<Tournament>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                tournaments.Add(Map(dto));
            }
        }
        return tournaments;
    }

    public async Task<Tournament?> AddTournamentAsync(Tournament tournament)
    {
        var dto = new EsportPlatform.API.Data.DTOs.TournamentDto.CreateTournamentDto(
            tournament.Name,
            tournament.StartDate,
            tournament.EndDate,
            tournament.GameId,
            tournament.Format,
            tournament.PrizePool,
            tournament.MaxTeams,
            tournament.Description,
            null,
            null
        );

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.TournamentDto.TournamentDto>(
            HttpMethod.Post, Endpoint, dto);

        return result is null ? null : Map(result);
    }

    private static Tournament Map(EsportPlatform.API.Data.DTOs.TournamentDto.TournamentDto dto) => new()
    {
        Id = dto.Id,
        Name = dto.Name,
        GameId = dto.GameId,
        Format = dto.Format,
        PrizePool = dto.PrizePool,
        MaxTeams = dto.MaxTeams,
        RegisteredTeams = dto.RegisteredTeams,
        StartDate = dto.DebutDate,
        EndDate = dto.FinDate,
        Status = dto.Status,
        Description = dto.Description
    };
}
