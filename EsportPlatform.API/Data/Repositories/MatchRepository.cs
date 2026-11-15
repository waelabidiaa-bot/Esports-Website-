



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


}