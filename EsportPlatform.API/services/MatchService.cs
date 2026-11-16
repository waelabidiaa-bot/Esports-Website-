using Microsoft.EntityFrameworkCore;

using EsportPlatform.API.Contracts;

using EsportPlatform.API.Data.DTOs.MatchDto;

using EsportPlatform.API.Interfaces.services.interfaces;

using EsportPlatform.API.Interfaces.repositories.interfaces;

using EsportPlatform.API.Models;

namespace EsportPlatform.API.services;

public class MatchService : IMatchService
{
    private readonly IMatchRepository _matchRespository;

    public MatchService(IMatchRepository matchRespository)
    {
        _matchRespository = matchRespository;
    }

    public async Task<List<MatchDto>> GetAllMatchesAsync()
    {
        var matches = await _matchRespository.GetAllAsync();
        return matches.Select(m => new MatchDto(m)).ToList();
    }

    public async Task<PagedResult<MatchDto>> GetAllMatchesPagedAsync(
        PaginationQuery pagination)
    {
        pagination ??= new PaginationQuery();

        var page = await _matchRespository.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize);

        return page.Map(m => new MatchDto(m));
    }

    public async Task<MatchDto?> GetMatchByIdAsync(int id)
    {
        var match = await _matchRespository.GetByIdAsync(id);
        return match is null ? null : new MatchDto(match);
    }

    public async Task<MatchDto?> CreateMatchAsync(CreateMatchDto match)
    {

        var tournament = await _matchRespository.GetTournamentByIdAsync(match.TournamentId);

        if (tournament is null)
            return null;

        if (match.TeamAId is not null && match.TeamAId == match.TeamBId)
            return null;

        if (!await TeamsBelongToGameAsync(match.TeamAId, match.TeamBId, tournament.GameId))
            return null;

        var newMatch = new Match
        {
            TournamentId = match.TournamentId,

            TeamAId = match.TeamAId,
            TeamBId = match.TeamBId,
            ScoreA = match.ScoreA,
            ScoreB = match.ScoreB,
            Stage = match.Stage,
            BestOf = match.BestOf,
            Status = match.Status
        };
        if (newMatch.TeamAId == newMatch.TeamBId)
        {
            return null;
        }

        await _matchRespository.AddAsync(newMatch);
        await _matchRespository.SaveChangesAsync();

        return new MatchDto(newMatch);
    }

    public async Task<MatchDto?> UpdateMatchAsync(UpdateMatchDto match, int id)
    {
        var existing = await _matchRespository.GetByIdAsync(id);
        if (existing is null)
        {
            return null;
        }

        var tournament = await _matchRespository.GetTournamentByIdAsync(match.TournamentId);
        if (tournament is null)
            return null;

        if (match.TeamAId is not null && match.TeamAId == match.TeamBId)
            return null;

        if (!await TeamsBelongToGameAsync(match.TeamAId, match.TeamBId, tournament.GameId))
            return null;

        existing.TournamentId = match.TournamentId;
        existing.GameId = tournament.GameId;
        existing.TeamAId = match.TeamAId;
        existing.TeamBId = match.TeamBId;
        existing.ScoreA = match.ScoreA;
        existing.ScoreB = match.ScoreB;
        existing.Stage = match.Stage;
        existing.BestOf = match.BestOf;
        existing.Status = match.Status;

        await _matchRespository.SaveChangesAsync();

        return new MatchDto(existing);
    }

    public async Task<bool> DeleteMatchAsync(int id)
    {
        var match = await _matchRespository.GetByIdAsync(id);
        if (match is null)
        {
            return false;
        }

        _matchRespository.Delete(match);
        await _matchRespository.SaveChangesAsync();

        return true;
    }

    public async Task<MatchDto?> SetStatusAsync(int id, string status)
    {
        var match = await _matchRespository.GetByIdAsync(id);
        if (match is null)
        {
            return null;
        }

        match.Status = status;
        await _matchRespository.SaveChangesAsync();

        return new MatchDto(match);
    }

    public async Task<MatchDto?> UpdateScoreAsync(int id, int scoreA, int scoreB)
    {
        var match = await _matchRespository.GetByIdAsync(id);
        if (match is null)
        {
            return null;
        }

        match.ScoreA = scoreA;
        match.ScoreB = scoreB;
        await _matchRespository.SaveChangesAsync();

        return new MatchDto(match);
    }

    private async Task<bool> TeamsBelongToGameAsync(int? teamAId, int? teamBId, int gameId)
    {
        var ids = new[] { teamAId, teamBId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return true;

        var count = await _matchRespository.CountValidTeamsAsync(ids, gameId);

        return count == ids.Count;
    }
}