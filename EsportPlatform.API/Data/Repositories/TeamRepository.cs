using EsportPlatform.API.Data;
using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EsportPlatform.API.Repositories;

public class TeamRepository : Repository<Team>, ITeamRepository
{
    public TeamRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistTeamByGameId(int gameid)
    {
        return await _context.Games.AnyAsync(t => t.Id == gameid);
    }

    public async Task<bool> ExistTeamByNameOrTag(string name, string tag)
    {
        return await _context.Teams.AnyAsync(t => t.Name == name || t.Tag == tag);
    }

    public async Task<bool> ExistTeamByNameOrTagExcludingId(int id, string name, string tag)
    {
        return await _context.Teams.AnyAsync(t => t.Id != id && (t.Name == name || t.Tag == tag));
    }

    public async Task<bool> ExistGameByTeamId(int gameid)
    {
        return await _context.Games.AnyAsync(g => g.Id == gameid);
    }

    public async Task AddPlayerAsync(Player player)
    {
        await _context.Players.AddAsync(player);
    }

    public async Task<Tournament?> GetEligibleTournamentForTeamAsync(int gameId)
    {
        return await _context.Tournaments
            .Where(t => t.GameId == gameId &&
                        (t.Status == "Open" || t.Status == "Upcoming") &&
                        t.RegisteredTeams < t.MaxTeams)
            .OrderBy(t => t.DebutDate)
            .FirstOrDefaultAsync();
    }
}