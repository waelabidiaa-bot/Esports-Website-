


using EsportPlatform.API.Interfaces.repositories.interfaces;
using EsportPlatform.API.Models;
using EsportPlatform.API.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EsportPlatform.API.Data.Repositories;

public class PlayerRespository:Repository<Player>,IPlayerRepository
{
    

    public PlayerRespository(AppDbContext context):base(context)
    {
       
    }
    
    public async Task<bool> IsPlayerExist(string gamertag)
    {
        return await _context.Players.AnyAsync(p=>p.Gamertag==gamertag);
    }
    public async Task<Team?> FindTeamByPlayer(int teamid)
    {
        return await _context.Teams.FindAsync(teamid);
    }

    public async Task<bool> FindDuplicate(int id,string gamertag)
    {
        return await _context.Players.AnyAsync(p => p.Id != id && p.Gamertag == gamertag);
    }
}