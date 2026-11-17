using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Contracts;
using EsportPlatform.API.Models;
using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Data.DTOs.GameDto;
using EsportPlatform.API.Repositories;

namespace EsportPlatform.API.Data.Repositories;

public class GameRepository : Repository<Game>,IGameRepository
{
    

    public GameRepository(AppDbContext context) : base(context)
    {
    }



    public async Task<List<Game>> GetActiveGamesAsync()
    {
        return await _context.Games
            .AsNoTracking()
            .Where(g => g.IsActive)
            .ToListAsync();
    }

    public async Task<PagedResult<Game>> GetActiveGamesPagedAsync(
        int pageNumber,
        int pageSize)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, PaginationQuery.MaxPageSize);

        var query = _context.Games
            .AsNoTracking();
            

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(g => g.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return PagedResult<Game>.Create(items, pageNumber, pageSize, totalCount);
    }

    public async Task<bool> IsGameExistAsync(string name)
    {
        return await _context.Games.AnyAsync(g => g.Name == name);
    }

    public async Task<bool> IsGameInUseAsync(int gameId)
    {
        // All four dependants use Restrict delete behavior, so a referenced game
        // must be cleared before it can be removed from the catalog.
        return await _context.Teams.AnyAsync(t => t.GameId == gameId)
            || await _context.Tournaments.AnyAsync(t => t.GameId == gameId)
            || await _context.Matches.AnyAsync(m => m.GameId == gameId)
            || await _context.Highlights.AnyAsync(h => h.GameId == gameId);
    }
}