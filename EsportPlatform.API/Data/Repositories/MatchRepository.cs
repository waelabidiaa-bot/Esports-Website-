



using EsportPlatform.API.Models;
using EsportPlatform.API.Interfaces.repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Repositories;

namespace EsportPlatform.API.Data.Repositories;

public class MatchRepository : Repository<Match>,IMatchRepository
{
  
   
   public MatchRepository(AppDbContext context ):base(context)
   {
    

        
    }
   
    public async Task<Tournament?> GetTournamentByIdAsync(int tournamentId)
    {
        return await _context.Tournaments
                            .AsNoTracking()
                            .FirstOrDefaultAsync(t => t.Id == tournamentId);
        
    }

    public async Task<int> CountValidTeamsAsync(List<int>ids,int gameId)
    {
        return await _context.Teams
        .Where(t => ids.Contains(t.Id) && t.GameId == gameId)
        .CountAsync();
    }

    public async Task<List<Team>> GetTeamsByIdsAsync(IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0)
        {
            return new List<Team>();
        }

        return await _context.Teams
            .AsNoTracking()
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
    }

    public async Task AddTournamentMembershipsAsync(
        int tournamentId,
        IReadOnlyCollection<int> teamIds)
    {
        if (teamIds.Count == 0)
        {
            return;
        }

        var existingTeamIds = await _context.TournamentTeams
            .AsNoTracking()
            .Where(tt => tt.TournamentId == tournamentId &&
                         teamIds.Contains(tt.TeamId))
            .Select(tt => tt.TeamId)
            .ToListAsync();

        var additions = teamIds
            .Distinct()
            .Where(id => !existingTeamIds.Contains(id))
            .Select(id => new TournamentTeam
            {
                TournamentId = tournamentId,
                TeamId = id,
                JoinedAt = DateTime.UtcNow
            })
            .ToList();

        if (additions.Count > 0)
        {
            await _context.TournamentTeams.AddRangeAsync(additions);
        }
    }


}