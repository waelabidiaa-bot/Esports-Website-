using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data.DTOs.PlayerDto;


namespace EsportPlatform.API.Interfaces.services.interfaces;

public interface IPlayerService
{
    public  Task<List<PlayerDto>> GetAllPlayersAsync();
    public Task<PagedResult<PlayerDto>> GetAllPlayersPagedAsync(PaginationQuery pagination);
    public  Task<PlayerDto?> CreatePlayerAsync(CreatePlayerDto playerDto);
    public Task<PlayerDto?> GetPlayerByIdAsync(int id);
    public Task<PlayerDto?> UpdatePlayerAsync(UpdatePlayerDto player, int id);
    public Task<bool> DeletePlayerAsync(int id);
    public Task<PlayerDto?> SetStatusAsync(int id, string status);


}