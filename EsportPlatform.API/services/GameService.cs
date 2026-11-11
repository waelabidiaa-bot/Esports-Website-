using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Data.DTOs.GameDto;
using EsportPlatform.API.Models;


namespace EsportPlatform.API.services;

public class GameService
{
    private readonly AppDbContext _context;

    public GameService(AppDbContext context) => _context = context;

    public async Task<List<GameDto>> GetAllGamesAsync()
        => await _context.Games
            .Where(g => g.IsActive)
            .Select(g => new GameDto
            {
                Id = g.Id,
                Name = g.Name,
                Genre = g.Genre,
                Description = g.Description,
                Platforms = g.Platforms,
                IsActive = g.IsActive,
                ImageData = g.ImageData
            })
            .ToListAsync();

    public async Task<GameDto?> GetGameByIdAsync(int id)
    {
        var game = await _context.Games
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);
        return game is null ? null : new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre,
            Description = game.Description,
            Platforms = game.Platforms,
            IsActive = game.IsActive,
            ImageData = game.ImageData
        };
    }

    public async Task<bool> CreateGameAsync(CreateGameDto dto)
    {
        if (await _context.Games.AnyAsync(g => g.Name == dto.Name))
            return false;

        _context.Games.Add(new Game
        {
            Name = dto.Name,
            Genre = dto.Genre,
            Description = dto.Description,
            Platforms = dto.Platforms,
            IsActive = true,
            ImageData = dto.ImageData
        });
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GameDto?> UpdateGameAsync(int id, UpdateGameDto dto)
    {
        var game = await _context.Games.FindAsync(id);
        if (game is null) return null;

        game.Name = dto.Name;
        game.Genre = dto.Genre;
        game.Description = dto.Description;
        game.Platforms = dto.Platforms;
        game.IsActive = dto.IsActive;
        game.ImageData = dto.ImageData;

        await _context.SaveChangesAsync();
        return new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre,
            Description = game.Description,
            Platforms = game.Platforms,
            IsActive = game.IsActive,
            ImageData = game.ImageData
        };
    }

    public async Task<GameDto?> ToggleGameAsync(int id)
    {
        var game = await _context.Games.FindAsync(id);
        if (game is null) return null;

        game.IsActive = !game.IsActive;
        await _context.SaveChangesAsync();
        return new GameDto
        {
            Id = game.Id,
            Name = game.Name,
            Genre = game.Genre,
            Description = game.Description,
            Platforms = game.Platforms,
            IsActive = game.IsActive,
            ImageData = game.ImageData
        };
    }
}   