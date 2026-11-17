using Microsoft.EntityFrameworkCore;

using EsportPlatform.API.Contracts;

using EsportPlatform.API.Data.DTOs.MatchDto;

using EsportPlatform.API.Interfaces.services.interfaces;

using EsportPlatform.API.Interfaces.repositories.interfaces;

using EsportPlatform.API.Models;

namespace EsportPlatform.API.services;

public class MatchService : IMatchService
{
    private const string ApprovedTeamStatus = "Approved";

    private static readonly HashSet<string> AllowedStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Upcoming", "Live", "Completed"
        };

    private static readonly HashSet<string> AllowedBestOfFormats =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Bo1", "Bo3", "Bo5", "Bo7"
        };

    private static readonly HashSet<string> ClosedTournamentStatuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Completed", "Finished", "Cancelled", "Archived"
        };

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

        if (!await IsScheduleValidAsync(
                tournament,
                match.GameId,
                match.TeamAId,
                match.TeamBId,
                match.ScoreA,
                match.ScoreB,
                match.MatchDate,
                match.BestOf,
                match.Status,
                match.Stage,
                excludeMatchId: null))
        {
            return null;
        }

        var newMatch = new Match
        {
            TournamentId = match.TournamentId,
            GameId = tournament.GameId,
            TeamAId = match.TeamAId,
            TeamBId = match.TeamBId,
            ScoreA = match.ScoreA,
            ScoreB = match.ScoreB,
            MatchDate = match.MatchDate,
            Stage = match.Stage.Trim(),
            BestOf = match.BestOf,
            Status = match.Status
        };

        await _matchRespository.AddAsync(newMatch);
        await AddTournamentMembershipsAsync(
            match.TournamentId,
            match.TeamAId,
            match.TeamBId);
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
        {
            return null;
        }

        if (!await IsScheduleValidAsync(
                tournament,
                match.GameId,
                match.TeamAId,
                match.TeamBId,
                match.ScoreA,
                match.ScoreB,
                match.MatchDate,
                match.BestOf,
                match.Status,
                match.Stage,
                excludeMatchId: id))
        {
            return null;
        }

        existing.TournamentId = match.TournamentId;
        existing.GameId = tournament.GameId;
        existing.TeamAId = match.TeamAId;
        existing.TeamBId = match.TeamBId;
        existing.ScoreA = match.ScoreA;
        existing.ScoreB = match.ScoreB;
        existing.MatchDate = match.MatchDate;
        existing.Stage = match.Stage.Trim();
        existing.BestOf = match.BestOf;
        existing.Status = match.Status;

        await AddTournamentMembershipsAsync(
            match.TournamentId,
            match.TeamAId,
            match.TeamBId);
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

    public async Task<MatchDto?> UpdateScoreAsync(int id, UpdateScoreDto score)
    {
        if (score.ScoreA < 0 || score.ScoreB < 0)
            return null;

        var match = await _matchRespository.GetByIdAsync(id);
        if (match is null)
        {
            return null;
        }

        match.ScoreA = score.ScoreA;
        match.ScoreB = score.ScoreB;

        _matchRespository.Update(match);
        await _matchRespository.SaveChangesAsync();

        return new MatchDto(match);
    }

    private async Task AddTournamentMembershipsAsync(
        int tournamentId,
        int? teamAId,
        int? teamBId)
    {
        var teamIds = new[] { teamAId, teamBId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        await _matchRespository.AddTournamentMembershipsAsync(
            tournamentId,
            teamIds);
    }

    private async Task<bool> IsScheduleValidAsync(
        Tournament tournament,
        int requestedGameId,
        int? teamAId,
        int? teamBId,
        int scoreA,
        int scoreB,
        DateTime matchDate,
        string bestOf,
        string status,
        string stage,
        int? excludeMatchId)
    {
        // Closed tournaments no longer accept new or edited matches.
        if (ClosedTournamentStatuses.Contains(tournament.Status ?? string.Empty))
            return false;

        if (!AllowedStatuses.Contains(status ?? string.Empty) ||
            !AllowedBestOfFormats.Contains(bestOf ?? string.Empty))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(stage))
            return false;

        if (scoreA < 0 || scoreB < 0)
            return false;

        if (requestedGameId != 0 && requestedGameId != tournament.GameId)
            return false;

        // A team can never be scheduled against itself.
        if (teamAId is not null && teamAId == teamBId)
            return false;

        var hasTeamA = teamAId.HasValue;
        var hasTeamB = teamBId.HasValue;

        // Either both sides are known, or this is a TBD upcoming slot.
        if (hasTeamA != hasTeamB)
            return false;

        var isUpcoming = string.Equals(
            status, "Upcoming", StringComparison.OrdinalIgnoreCase);

        if (!isUpcoming && !(hasTeamA && hasTeamB))
            return false;

        if (!IsWithinTournamentDates(tournament, matchDate))
            return false;

        var teamIds = new[] { teamAId, teamBId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (teamIds.Count > 0)
        {
            var teams = await _matchRespository.GetTeamsByIdsAsync(teamIds);

            if (teams.Count != teamIds.Count)
                return false;

            if (teams.Any(t => !string.Equals(
                    t.Status,
                    ApprovedTeamStatus,
                    StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            // Both teams must belong to the tournament's game.
            if (teams.Any(t => t.GameId != tournament.GameId))
                return false;
        }

        if (!(hasTeamA && hasTeamB))
            return true;

        var teamA = teamAId!.Value;
        var teamB = teamBId!.Value;
        var stageName = stage.Trim();

        var scheduledMatches = (await _matchRespository.GetAllAsync())
            .Where(m => m.Id != excludeMatchId);

        foreach (var existingMatch in scheduledMatches)
        {
            // A pairing can only appear once per stage of a tournament.
            if (existingMatch.TournamentId == tournament.Id &&
                IsSameTeamPair(existingMatch.TeamAId, existingMatch.TeamBId, teamA, teamB) &&
                string.Equals(
                    (existingMatch.Stage ?? string.Empty).Trim(),
                    stageName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // A team cannot be in two matches scheduled at the same time.
            if (existingMatch.MatchDate == matchDate &&
                (existingMatch.TeamAId == teamA ||
                 existingMatch.TeamBId == teamA ||
                 existingMatch.TeamAId == teamB ||
                 existingMatch.TeamBId == teamB))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsWithinTournamentDates(
        Tournament tournament,
        DateTime matchDate)
    {
        if (matchDate == default)
            return false;

        var date = matchDate.Date;

        if (tournament.DebutDate != default &&
            date < tournament.DebutDate.Date)
        {
            return false;
        }

        if (tournament.FinDate != default &&
            date > tournament.FinDate.Date)
        {
            return false;
        }

        return true;
    }

    private static bool IsSameTeamPair(
        int? teamAId,
        int? teamBId,
        int firstTeamId,
        int secondTeamId)
    {
        return (teamAId == firstTeamId && teamBId == secondTeamId) ||
               (teamAId == secondTeamId && teamBId == firstTeamId);
    }
}
