using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;

namespace EsportPlatform.API.services;

public class TournamentService
{
    private readonly AppDbContext _context;

    public TournamentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tournament>> GetAllTournamentsAsync()
    {
        return await _context.Tournaments.ToListAsync();
    }

    public async Task<Tournament?> GetTournamentByIdAsync(int id)
    {
        return await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tournament?> CreateTournamentAsync(Tournament tournament)
    {
        var gameExists = await _context.Games.AnyAsync(g => g.Id == tournament.GameId);
        var duplicate = await _context.Tournaments.AnyAsync(t => t.Name == tournament.Name);

        if (!gameExists || duplicate)
        {
            return null;
        }

        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync();
        return tournament;
    }

    public async Task<Tournament?> UpdateTournamentAsync(Tournament tournament, int id)
    {
        var existing = await _context.Tournaments.FindAsync(id);
        if (existing is null)
        {
            return null;
        }

        var gameExists = await _context.Games.AnyAsync(g => g.Id == tournament.GameId);
        var duplicate = await _context.Tournaments
            .AnyAsync(t => t.Id != id && t.Name == tournament.Name);

        if (!gameExists || duplicate)
        {
            return null;
        }

        existing.Name = tournament.Name;
        existing.DebutDate = tournament.DebutDate;
        existing.FinDate = tournament.FinDate;
        existing.GameId = tournament.GameId;
        existing.Format = tournament.Format;
        existing.PrizePool = tournament.PrizePool;
        existing.MaxTeams = tournament.MaxTeams;
        existing.RegisteredTeams = tournament.RegisteredTeams;
        existing.Status = tournament.Status;
        existing.Description = tournament.Description;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteTournamentAsync(int id)
    {
        var tournament = await _context.Tournaments.FindAsync(id);
        if (tournament is null)
        {
            return false;
        }

        _context.Tournaments.Remove(tournament);
        await _context.SaveChangesAsync();
        return true;
    }
}
