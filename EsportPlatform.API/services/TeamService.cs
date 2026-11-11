using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;

namespace EsportPlatform.API.services;

public class TeamService
{
    private readonly AppDbContext _context;

    public TeamService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Team>> GetAllTeamsAsync()
    {
        return await _context.Teams.ToListAsync();
    }

    public async Task<Team?> GetTeamByIdAsync(int id)
    {
        return await _context.Teams.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Team?> CreateTeamAsync(Team team)
    {
        var gameExists = await _context.Games.AnyAsync(g => g.Id == team.GameId);
        var duplicate = await _context.Teams.AnyAsync(t => t.Name == team.Name || t.Tag == team.Tag);

        if (!gameExists || duplicate)
        {
            return null;
        }

        team.CreatedAt = team.CreatedAt == default ? DateTime.UtcNow : team.CreatedAt;
        _context.Teams.Add(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task<Team?> UpdateTeamAsync(Team team, int id)
    {
        var existing = await _context.Teams.FindAsync(id);
        if (existing is null)
        {
            return null;
        }

        var gameExists = await _context.Games.AnyAsync(g => g.Id == team.GameId);
        var duplicate = await _context.Teams
            .AnyAsync(t => t.Id != id && (t.Name == team.Name || t.Tag == team.Tag));

        if (!gameExists || duplicate)
        {
            return null;
        }

        existing.Name = team.Name;
        existing.Tag = team.Tag;
        existing.GameId = team.GameId;
        existing.Region = team.Region;
        existing.Captain = team.Captain;
        existing.RosterSize = team.RosterSize;
        existing.MaxRoster = team.MaxRoster;
        existing.Points = team.Points;
        existing.Status = team.Status;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteTeamAsync(int id)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team is null)
        {
            return false;
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Team?> SetStatusAsync(int id, string status)
    {
        var team = await _context.Teams.FindAsync(id);
        if (team is null)
        {
            return null;
        }

        team.Status = status;
        await _context.SaveChangesAsync();
        return team;
    }

    /// <summary>
    /// Creates a pending team together with its roster and reserves a slot in the
    /// earliest open tournament for that game.
    /// </summary>
    public async Task<Team?> RegisterTeamAsync(Team team, IEnumerable<Player> roster)
    {
        var gameExists = await _context.Games.AnyAsync(g => g.Id == team.GameId);
        var duplicate = await _context.Teams.AnyAsync(t => t.Name == team.Name || t.Tag == team.Tag);

        if (!gameExists || duplicate)
        {
            return null;
        }

        var players = roster.ToList();
        team.Status = "Pending";
        team.CreatedAt = DateTime.UtcNow;
        team.RosterSize = players.Count;

        _context.Teams.Add(team);
        await _context.SaveChangesAsync();

        foreach (var player in players)
        {
            player.TeamId = team.Id;
            _context.Players.Add(player);
        }

        var tournament = await _context.Tournaments
            .Where(t => t.GameId == team.GameId &&
                        (t.Status == "Open" || t.Status == "Upcoming") &&
                        t.RegisteredTeams < t.MaxTeams)
            .OrderBy(t => t.DebutDate)
            .FirstOrDefaultAsync();

        if (tournament is not null)
        {
            tournament.RegisteredTeams++;
        }

        await _context.SaveChangesAsync();
        return team;
    }

    /// <summary>
    /// Adds a player to an existing roster and keeps the cached roster size in sync.
    /// </summary>
    public async Task<Player?> AddPlayerToTeamAsync(int teamId, Player player)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team is null || team.RosterSize >= team.MaxRoster)
        {
            return null;
        }

        player.TeamId = teamId;
        _context.Players.Add(player);
        team.RosterSize++;

        await _context.SaveChangesAsync();
        return player;
    }
}
