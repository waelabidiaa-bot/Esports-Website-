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

    /// <summary>
    /// Deletes the whole match record. The API returns 204 NoContent on success.
    /// </summary>
    public async Task DeleteMatchAsync(int id)
    {
        await Api.SendAsync<object>(HttpMethod.Delete, $"{Endpoint}/{id}");
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
    /// Changes the selected team's score by the supplied delta without
    /// resetting the other team's score. Scores never drop below zero.
    /// </summary>
    public async Task<GameMatch?> AdjustScoreAsync(int id, bool teamA, int delta = 1)
    {
        var current = await GetMatchAsync(id);
        if (current is null)
            return null;

        var scoreA = teamA ? Math.Max(0, current.ScoreA + delta) : current.ScoreA;
        var scoreB = teamA ? current.ScoreB : Math.Max(0, current.ScoreB + delta);

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
