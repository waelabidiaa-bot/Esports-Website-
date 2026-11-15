using EsportPlatform.API.Data.DTOs.GameDto;
using EsportPlatform.API.Models; 

namespace EsportPlatform.API.Interfaces.repositories.interfaces;

public interface IGameRepository:IRepository<Game>
{
    
    Task<bool> IsGameExistAsync(string name);
    Task<List<Game>> GetActiveGamesAsync();
}