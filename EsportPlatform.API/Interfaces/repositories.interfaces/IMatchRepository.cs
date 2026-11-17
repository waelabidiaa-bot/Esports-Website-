


using EsportPlatform.API.Models;


namespace EsportPlatform.API.Interfaces.repositories.interfaces;


public interface IMatchRepository:IRepository<Match>

{
    Task<Tournament?> GetTournamentByIdAsync(int tournamentId);
    Task<int> CountValidTeamsAsync(List<int> ids,int gameId);
    Task<List<Team>> GetTeamsByIdsAsync(IReadOnlyCollection<int> ids);
    Task AddTournamentMembershipsAsync(int tournamentId, IReadOnlyCollection<int> teamIds);
    


    

   
}