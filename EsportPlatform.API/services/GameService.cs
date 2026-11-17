using Microsoft.EntityFrameworkCore;

using EsportPlatform.API.Contracts;
using EsportPlatform.API.Data.DTOs.GameDto;

using EsportPlatform.API.Interfaces.services.interfaces;

using EsportPlatform.API.Interfaces.repositories.interfaces;


namespace EsportPlatform.API.services;

public class GameService : IGameService
{
    
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository) 
    { 
        

        _gameRepository = gameRepository;

    }

    public async Task<List<GameDto>> GetAllGamesAsync()
        {
           var games= await _gameRepository.GetActiveGamesAsync();
           
           return games.Select(g => new GameDto(g)).ToList();
        }

    public async Task<PagedResult<GameDto>> GetAllGamesPagedAsync(
        PaginationQuery pagination)
    {
        pagination ??= new PaginationQuery();

        var page = await _gameRepository.GetActiveGamesPagedAsync(
            pagination.PageNumber,
            pagination.PageSize);

        return page.Map(g => new GameDto(g));
    }

    public async Task<GameDto?> GetGameByIdAsync(int id)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        
        return game is null ? null : new GameDto(game);
    }

    public async Task<GameDto?> CreateGameAsync(CreateGameDto dto)
    {
        
        var nameExists = await _gameRepository.IsGameExistAsync(dto.Name);
        if (nameExists)
        {
            return null;
        }

        // DTO helper method to generate the entity (mapping logic inside the dto)
        var newGame = dto.ToEntity();

        await _gameRepository.AddAsync(newGame);
        await _gameRepository.SaveChangesAsync();
        
        return new GameDto(newGame);
    }

    public async Task<GameDto?> UpdateGameAsync(int id, UpdateGameDto dto)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        if (game is null) return null;

        // Use the ApplyTo helper instead of manual field assignments
        dto.ApplyTo(game);

        await _gameRepository.SaveChangesAsync();
        return new GameDto(game);
    }

    public async Task<GameDto?> ToggleGameAsync(int id)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        if (game is null) return null;

        game.IsActive = !game.IsActive;
        await _gameRepository.SaveChangesAsync();
        
        return new GameDto(game);
    }

    /// <summary>
    /// Deletes a game only when no teams, tournaments, matches or highlights
    /// still reference it. Those relationships use Restrict delete behavior.
    /// </summary>
    public async Task<DeleteResult> DeleteGameAsync(int id)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        if (game is null)
        {
            return DeleteResult.NotFound;
        }

        if (await _gameRepository.IsGameInUseAsync(id))
        {
            return DeleteResult.Conflict;
        }

        _gameRepository.Delete(game);
        await _gameRepository.SaveChangesAsync();

        return DeleteResult.Success;
    }
}