using Microsoft.EntityFrameworkCore;
using EsportPlatform.API.Data;
using EsportPlatform.API.Models;

using EsportPlatform.API.Data.DTOs.HighlightDto;
namespace EsportPlatform.API.services;

public class HighlightService
{
    private readonly AppDbContext _context;

    public HighlightService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HighlightDto>> GetAllHighlightsAsync()
    {
        return await _context.Highlights
                            .Select(g=>new HighlightDto(g))
                            .ToListAsync();
    }











public async Task<HighlightDto?> GetHighlightByIdAsync(int id)
{
    var h = await _context.Highlights.FirstOrDefaultAsync(h => h.Id == id);
    return h is null ? null : new HighlightDto(h);
}   









    public async Task<HighlightDto?> CreateHighlightAsync(CreateHighlightDto dto)
{
    if (!await _context.Games.AnyAsync(g => g.Id == dto.GameId))
        return null;

    var highlight = new Highlight
    {
        Title = dto.Title,
        GameId = dto.GameId,
        MatchLabel = dto.MatchLabel,
        VideoUrl = dto.VideoUrl,
        Duration = dto.Duration,
        Views = 0,
        PublishedAt = DateTime.UtcNow
    };

    _context.Highlights.Add(highlight);
    await _context.SaveChangesAsync();
    return new HighlightDto(highlight);
}   

   public async Task<HighlightDto?> UpdateHighlightAsync(int id, UpdateHighlightDto dto)
{
    var existing = await _context.Highlights.FindAsync(id);
    if (existing is null) return null;
    if (!await _context.Games.AnyAsync(g => g.Id == dto.GameId)) return null;

    existing.Title = dto.Title;
    existing.GameId = dto.GameId;
    existing.MatchLabel = dto.MatchLabel;
    existing.VideoUrl = dto.VideoUrl;
    existing.Duration = dto.Duration;

    await _context.SaveChangesAsync();
    return new HighlightDto(existing);
}   










    public async Task<bool> DeleteHighlightAsync(int id)
    {
        var highlight = await _context.Highlights.FindAsync(id);
        if (highlight is null)
        {
            return false;
        }

        _context.Highlights.Remove(highlight);
        await _context.SaveChangesAsync();
        return true;
    }
}
