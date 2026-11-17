using EsportPlatform.API.Contracts;
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

    public async Task<List<Team>> GetAllWithTournamentsAsync()
    {
        return await _context.Teams
            .AsNoTracking()
            .Include(t => t.TournamentTeams)
            .ThenInclude(tt => tt.Tournament)
            .OrderBy(t => t.Id)
            .ToListAsync();
    }

    public async Task<PagedResult<Team>> GetPagedWithTournamentsAsync(
        int pageNumber,
        int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, PaginationQuery.MaxPageSize);

        var query = _context.Teams
            .AsNoTracking()
            .Include(t => t.TournamentTeams)
            .ThenInclude(tt => tt.Tournament);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Team>.Create(items, pageNumber, pageSize, totalCount);
    }

    public async Task<Team?> GetByIdWithTournamentsAsync(int id)
    {
        return await _context.Teams
            .Include(t => t.TournamentTeams)
            .ThenInclude(tt => tt.Tournament)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<int> CountPendingPlayersForTeamAsync(int teamId)
    {
        return await _context.Players.CountAsync(p =>
            p.TeamId == teamId && p.ApprovalStatus == "Pending");
    }

    public async Task<List<Tournament>> GetTournamentsForTeamAsync(int teamId)
    {
        return await _context.TournamentTeams
            .Where(tt => tt.TeamId == teamId)
            .Select(tt => tt.Tournament)
            .Distinct()
            .ToListAsync();
    }

    public async Task AddTournamentMembershipAsync(int tournamentId, int teamId)
    {
        var exists = await _context.TournamentTeams
            .AnyAsync(tt => tt.TournamentId == tournamentId && tt.TeamId == teamId);

        if (exists)
        {
            return;
        }

        await _context.TournamentTeams.AddAsync(new TournamentTeam
        {
            TournamentId = tournamentId,
            TeamId = teamId,
            JoinedAt = DateTime.UtcNow
        });
    }
}