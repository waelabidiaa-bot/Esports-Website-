

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data.DTOs.MatchDto;
using EsportPlatform.API.Models;


namespace EsportPlatform.API.Interfaces.services.interfaces;

public interface IMatchService
{
    public Task<List<MatchDto>> GetAllMatchesAsync();
    public Task<PagedResult<MatchDto>> GetAllMatchesPagedAsync(PaginationQuery pagination);
    public Task<MatchDto?> GetMatchByIdAsync(int id);
    public Task<MatchDto?> CreateMatchAsync(CreateMatchDto match);
    public Task<MatchDto?> UpdateMatchAsync(UpdateMatchDto match, int id);
    public Task<bool> DeleteMatchAsync(int id);
    public Task<MatchDto?> SetStatusAsync(int id, string status);
    public  Task<MatchDto?> UpdateScoreAsync(int id, int scoreA, int scoreB);
    

}