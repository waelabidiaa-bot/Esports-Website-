using EsportPlatform.API.Data.DTOs.TeamDto;
using EsportPlatform.API.Data.DTOs.PlayerDto;



namespace EsportPlatform.API.Interfaces.services.interfaces;

public interface ITeamService
{
   public Task<List<TeamDto>> GetAllTeamsAsync();
   public  Task<TeamDto?> GetTeamByIdAsync(int id);
   public Task<TeamDto?> CreateTeamAsync(CreateTeamDto teamDto);
   public  Task<TeamDto?> UpdateTeamAsync(UpdateTeamDto teamDto, int id);
   public  Task<bool> DeleteTeamAsync(int id);
   public  Task<TeamDto?> SetStatusAsync(int id, string status);
   public Task<TeamDto?> RegisterTeamAsync(CreateTeamDto teamDto, IEnumerable<CreatePlayerDto> roster);
   public  Task<PlayerDto?> AddPlayerToTeamAsync(int teamId, CreatePlayerDto playerDto);




}