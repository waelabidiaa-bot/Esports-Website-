using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class MatchApiService : BaseApiService
{
    private const string Endpoint = "api/match";

    public MatchApiService(ApiClient api) : base(api) { }

    public async Task<List<GameMatch>> GetMatchesAsync()
    {
        var dtos = await Api.GetAllAsync<EsportPlatform.API.Data.DTOs.MatchDto.MatchDto>(Endpoint);
        var matches = new List<GameMatch>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                var match = new GameMatch
                {
                    Id = dto.Id,
                    TournamentId = dto.TournamentId,
                    GameId = dto.GameId,
                    TeamAId = dto.TeamAId,
                    TeamBId = dto.TeamBId,
                    ScoreA = dto.ScoreA,
                    ScoreB = dto.ScoreB,
                    Status = dto.Status,
                    Stage = dto.Stage,
                    BestOf = dto.BestOf,
                    ScheduledAt = dto.MatchDate
                };
                matches.Add(match);
            }
        }
        return matches;
    }

    public async Task<GameMatch?> AddMatchAsync(GameMatch match)
    {
        var dto = new EsportPlatform.API.Data.DTOs.MatchDto.CreateMatchDto(
            TournamentId: match.TournamentId,
            GameId: match.GameId,
            TeamAId: match.TeamAId,
            TeamBId: match.TeamBId,
            ScoreA: match.ScoreA,
            ScoreB: match.ScoreB,
            BestOf: match.BestOf,
            Status: match.Status,
            Stage: match.Stage);

        var result = await Api.SendAsync<EsportPlatform.API.Data.DTOs.MatchDto.MatchDto>(
            HttpMethod.Post, Endpoint, dto);

        if (result is null) return null;

        return new GameMatch
        {
            Id = result.Id,
            TournamentId = result.TournamentId,
            GameId = result.GameId,
            TeamAId = result.TeamAId,
            TeamBId = result.TeamBId,
            ScoreA = result.ScoreA,
            ScoreB = result.ScoreB,
            Status = result.Status,
            Stage = result.Stage,
            BestOf = result.BestOf,
            ScheduledAt = result.MatchDate
        };
    }

    public async Task SetMatchLiveAsync(int id)
    {
        await Api.SendAsync<object>(HttpMethod.Patch, $"{Endpoint}/{id}/status", new { status = "Live" });
    }

    public async Task FinalizeMatchAsync(int id)
    {
        await Api.SendAsync<object>(HttpMethod.Patch, $"{Endpoint}/{id}/status", new { status = "Completed" });
    }

    public async Task AdjustScoreAsync(int id, bool teamA)
    {
        var scoreA = teamA ? 1 : 0;
        var scoreB = teamA ? 0 : 1;

        await Api.SendAsync<object>(
            HttpMethod.Patch, $"{Endpoint}/{id}/score", new { scoreA, scoreB });
    }
}