using EsportPlatform.API.Data;
using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EsportPlatform.API.Repositories;

public class TournamentRepository : Repository<Tournament>, ITournamentRepository
{
    public TournamentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistGameByIdAsync(int gameId)
    {
        return await _context.Games.AnyAsync(g => g.Id == gameId);
    }

    public async Task<bool> ExistTournamentByNameAsync(string name)
    {
        return await _context.Tournaments.AnyAsync(t => t.Name == name);
    }

    public async Task<bool> ExistTournamentByNameExcludingIdAsync(int id, string name)
    {
        return await _context.Tournaments.AnyAsync(t => t.Id != id && t.Name == name);
    }
}