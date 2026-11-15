



using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Models;
using EsportPlatform.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EsportPlatform.API.Data.Repositories;

public class HighlightRepository: Repository<Highlight>,IHighlightRepository
{
    

    public HighlightRepository(AppDbContext context) : base(context)
    {
        
    }



   public async Task<bool> ExistByGameIdAsync(int gameid)
    {
        return await _context.Highlights.AnyAsync(t=>t.GameId==gameid);
    }
   

    

}