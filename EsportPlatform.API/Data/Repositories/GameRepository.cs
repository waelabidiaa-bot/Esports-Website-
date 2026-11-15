using Microsoft.EntityFrameworkCore;
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

    public async Task<bool> IsGameExistAsync(string name)
    {
        return await _context.Games.AnyAsync(g => g.Name == name);
    }
}