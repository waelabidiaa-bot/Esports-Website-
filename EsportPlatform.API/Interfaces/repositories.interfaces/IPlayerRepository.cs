


using EsportPlatform.API.Models;

namespace EsportPlatform.API.Interfaces.repositories.interfaces;

public interface IPlayerRepository:IRepository<Player>
{
    
    Task<bool> IsPlayerExist(string gamertag);
    Task<Team?> FindTeamByPlayer(int teamid);
    Task<bool> FindDuplicate(int id,string gamertag);
    
}