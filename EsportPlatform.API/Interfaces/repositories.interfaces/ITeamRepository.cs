

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Models;

namespace EsportPlatform.API.Interfaces.repositories.interfaces;


public interface ITeamRepository:IRepository<Team>
{
    Task<bool> ExistTeamByGameId(int gameid);
    Task<bool> ExistTeamByNameOrTag(string name,string tag);
    Task<bool> ExistGameByTeamId(int gameid);
    Task<bool> ExistTeamByNameOrTagExcludingId(int id, string name, string tag);

    Task AddPlayerAsync(Player player);
    Task<Tournament?> GetEligibleTournamentForTeamAsync(int gameId);

    Task<List<Team>> GetAllWithTournamentsAsync();
    Task<PagedResult<Team>> GetPagedWithTournamentsAsync(int pageNumber, int pageSize);
    Task<Team?> GetByIdWithTournamentsAsync(int id);
    Task AddTournamentMembershipAsync(int tournamentId, int teamId);

}