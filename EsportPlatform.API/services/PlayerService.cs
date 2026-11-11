using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;

namespace EsportPlatform.API.services;

public class PlayerService
{
    private readonly AppDbContext _context;

    public PlayerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Player>> GetAllPlayersAsync()
    {
        return await _context.Players.ToListAsync();
    }

    public async Task<Player?> GetPlayerByIdAsync(int id)
    {
        return await _context.Players.FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Player?> CreatePlayerAsync(Player player)
    {
        var duplicate = await _context.Players.AnyAsync(p => p.Gamertag == player.Gamertag);
        if (duplicate)
        {
            return null;
        }

        if (player.TeamId is not null)
        {
            var team = await _context.Teams.FindAsync(player.TeamId);
            if (team is null || team.RosterSize >= team.MaxRoster)
            {
                return null;
            }

            team.RosterSize++;
        }

        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task<Player?> UpdatePlayerAsync(Player player, int id)
    {
        var existing = await _context.Players.FindAsync(id);
        if (existing is null)
        {
            return null;
        }

        var duplicate = await _context.Players
            .AnyAsync(p => p.Id != id && p.Gamertag == player.Gamertag);
        if (duplicate)
        {
            return null;
        }

        if (player.TeamId != existing.TeamId)
        {
            if (existing.TeamId is not null)
            {
                var oldTeam = await _context.Teams.FindAsync(existing.TeamId);
                if (oldTeam is not null && oldTeam.RosterSize > 0)
                {
                    oldTeam.RosterSize--;
                }
            }

            if (player.TeamId is not null)
            {
                var newTeam = await _context.Teams.FindAsync(player.TeamId);
                if (newTeam is null || newTeam.RosterSize >= newTeam.MaxRoster)
                {
                    return null;
                }

                newTeam.RosterSize++;
            }
        }

        existing.Gamertag = player.Gamertag;
        existing.FullName = player.FullName;
        existing.Email = player.Email;
        existing.TeamId = player.TeamId;
        existing.Role = player.Role;
        existing.Country = player.Country;
        existing.Status = player.Status;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeletePlayerAsync(int id)
    {
        var player = await _context.Players.FindAsync(id);
        if (player is null)
        {
            return false;
        }

        if (player.TeamId is not null)
        {
            var team = await _context.Teams.FindAsync(player.TeamId);
            if (team is not null && team.RosterSize > 0)
            {
                team.RosterSize--;
            }
        }

        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Player?> SetStatusAsync(int id, string status)
    {
        var player = await _context.Players.FindAsync(id);
        if (player is null)
        {
            return null;
        }

        player.Status = status;
        await _context.SaveChangesAsync();
        return player;
    }
}
