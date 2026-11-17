using EsportPlatform.API.Data.DTOs.MatchDto;
using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

public class MatchApiService : BaseApiService
{
    private const string Endpoint = "api/match";

    public MatchApiService(ApiClient api) : base(api) { }

    public async Task<List<GameMatch>> GetMatchesAsync()
    {
        var dtos = await Api.GetAllAsync<MatchDto>(Endpoint);
        var matches = new List<GameMatch>();
        if (dtos != null)
        {
            foreach (var dto in dtos)
            {
                matches.Add(ToGameMatch(dto));
            }
        }
        return matches;
    }

    public async Task<GameMatch?> GetMatchAsync(int id)
    {
        var dto = await Api.GetAsync<MatchDto>($"{Endpoint}/{id}");
        return dto is null ? null : ToGameMatch(dto);
    }

    public async Task<GameMatch?> AddMatchAsync(GameMatch match)
    {
        var dto = new CreateMatchDto(
            TournamentId: match.TournamentId,
            GameId: match.GameId,
            TeamAId: match.TeamAId,
            TeamBId: match.TeamBId,
            ScoreA: match.ScoreA,
            ScoreB: match.ScoreB,
            MatchDate: match.ScheduledAt,
            BestOf: match.BestOf,
            Status: match.Status,
            Stage: match.Stage);

        var result = await Api.SendAsync<MatchDto>(
            HttpMethod.Post, Endpoint, dto);

        return result is null ? null : ToGameMatch(result);
    }

    public async Task SetMatchLiveAsync(int id)
    {
        await Api.SendAsync<object>(HttpMethod.Patch, $"{Endpoint}/{id}/status", new { status = "Live" });
    }

    public async Task FinalizeMatchAsync(int id)
    {
        await Api.SendAsync<object>(HttpMethod.Patch, $"{Endpoint}/{id}/status", new { status = "Completed" });
    }

    /// <summary>
    /// Replaces the score of a match with the supplied values.
    /// </summary>
    public async Task<GameMatch?> UpdateScoreAsync(int id, int scoreA, int scoreB)
    {
        return await UpdateScoreAsync(id, new UpdateScoreDto(scoreA, scoreB));
    }

    /// <summary>
    /// Replaces the score of a match with the supplied values.
    /// </summary>
    public async Task<GameMatch?> UpdateScoreAsync(int id, UpdateScoreDto score)
    {
        var result = await Api.SendAsync<MatchDto>(
            HttpMethod.Patch, $"{Endpoint}/{id}/score", score);

        return result is null ? null : ToGameMatch(result);
    }

    /// <summary>
    /// Adds a point to the selected team without resetting the other team's score.
    /// </summary>
    public async Task<GameMatch?> AdjustScoreAsync(int id, bool teamA)
    {
        var current = await GetMatchAsync(id);
        if (current is null)
            return null;

        var scoreA = teamA ? current.ScoreA + 1 : current.ScoreA;
        var scoreB = teamA ? current.ScoreB : current.ScoreB + 1;

        return await UpdateScoreAsync(id, scoreA, scoreB);
    }

    private static GameMatch ToGameMatch(MatchDto dto)
    {
        return new GameMatch
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
    }
}
