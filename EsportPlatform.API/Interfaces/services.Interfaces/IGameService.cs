using EsportPlatform.API.Contracts;
using EsportPlatform.API.services;

using EsportPlatform.API.Data.DTOs.GameDto;

namespace EsportPlatform.API.Interfaces.services.interfaces;


public interface IGameService
{
    public  Task<List<GameDto>> GetAllGamesAsync();
    public Task<PagedResult<GameDto>> GetAllGamesPagedAsync(PaginationQuery pagination);
    public  Task<GameDto?> GetGameByIdAsync(int id);
    public  Task<GameDto?> CreateGameAsync(CreateGameDto dto);
    public  Task<GameDto?> UpdateGameAsync(int id, UpdateGameDto dto);
    public  Task<GameDto?> ToggleGameAsync(int id);


    
    
}