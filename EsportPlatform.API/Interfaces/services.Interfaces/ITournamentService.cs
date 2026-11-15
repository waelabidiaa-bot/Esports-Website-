using EsportPlatform.API.Data.DTOs.TournamentDto;




namespace EsportPlatform.API.Interfaces.services.interfaces;

public interface ITournamentService
{
    public  Task<List<TournamentDto>> GetAllTournamentsAsync();
    public Task<TournamentDto?> GetTournamentByIdAsync(int id);
    public  Task<TournamentDto?> CreateTournamentAsync(CreateTournamentDto tournament);
     public Task<TournamentDto?> UpdateTournamentAsync(UpdateTournamentDto tournament, int id);
     public Task<bool> DeleteTournamentAsync(int id);



}