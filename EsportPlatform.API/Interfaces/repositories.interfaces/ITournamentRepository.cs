using EsportPlatform.API.Models;

namespace EsportPlatform.API.Interfaces.repositories.interfaces;

public interface ITournamentRepository : IRepository<Tournament>
{
    Task<bool> ExistGameByIdAsync(int gameId);
    Task<bool> ExistTournamentByNameAsync(string name);
    Task<bool> ExistTournamentByNameExcludingIdAsync(int id, string name);
}