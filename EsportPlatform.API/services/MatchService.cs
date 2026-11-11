using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;
using EsportPlatform.API.Data.DTOs.MatchDto;

namespace EsportPlatform.API.services;

public class MatchService
{
    private readonly AppDbContext _context;

    public MatchService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<MatchDto>> GetAllMatchesAsync()
    {
        return await _context.Matches
            .Select(m => new MatchDto(m))
            .ToListAsync();
    }








    public async Task<MatchDto?> GetMatchByIdAsync(int id)
    {
        var match = await _context.Matches
            .FirstOrDefaultAsync(m => m.Id == id);

        return match is null ? null : new MatchDto(match);
    }










    public async Task<MatchDto?> CreateMatchAsync(CreateMatchDto match)
    {
        var tournament = await _context.Tournaments
            .FindAsync(match.TournamentId);

        if (tournament is null)
            return null;

        if (match.TeamAId is not null &&
            match.TeamAId == match.TeamBId)
            return null;

        if (!await TeamsBelongToGameAsync(
            match.TeamAId,
            match.TeamBId,
            tournament.GameId))
            return null;

        var newMatch = new Match
        {
            TournamentId = match.TournamentId,
            GameId = tournament.GameId,
            TeamAId = match.TeamAId,
            TeamBId = match.TeamBId,
            ScoreA = match.ScoreA,
            ScoreB = match.ScoreB,
            Stage = match.Stage,
            BestOf = match.BestOf,
            Status = match.Status
        };

        _context.Matches.Add(newMatch);

        await _context.SaveChangesAsync();

        return new MatchDto(newMatch);
    }







    public async Task<MatchDto?> UpdateMatchAsync(
        UpdateMatchDto match, int id)
    {
        var existing = await _context.Matches.FindAsync(id);

        if (existing is null)
            return null;

        var tournament = await _context.Tournaments
            .FindAsync(match.TournamentId);

        if (tournament is null)
            return null;

        if (match.TeamAId is not null &&
            match.TeamAId == match.TeamBId)
            return null;

        if (!await TeamsBelongToGameAsync(
            match.TeamAId,
            match.TeamBId,
            tournament.GameId))
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

        await _context.SaveChangesAsync();

        return new MatchDto(existing);
    }











    public async Task<bool> DeleteMatchAsync(int id)
    {
        var match = await _context.Matches.FindAsync(id);

        if (match is null)
            return false;

        _context.Matches.Remove(match);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Match?> SetStatusAsync(int id, string status)
    {
        var match = await _context.Matches.FindAsync(id);

        if (match is null)
            return null;

        match.Status = status;

        await _context.SaveChangesAsync();

        return match;
    }









    public async Task<Match?> UpdateScoreAsync(
        int id, int scoreA, int scoreB)
    {
        var match = await _context.Matches.FindAsync(id);

        if (match is null)
            return null;

        match.ScoreA = scoreA;
        match.ScoreB = scoreB;

        await _context.SaveChangesAsync();

        return match;
    }







    

    private async Task<bool> TeamsBelongToGameAsync(
        int? teamAId, int? teamBId, int gameId)
    {
        var ids = new[] { teamAId, teamBId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return true;

        var count = await _context.Teams.CountAsync(
            t => ids.Contains(t.Id) && t.GameId == gameId);

        return count == ids.Count;
    }
}