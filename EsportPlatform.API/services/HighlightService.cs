using Microsoft.EntityFrameworkCore;

using EsportPlatform.API.Data;

using EsportPlatform.API.Data.DTOs.HighlightDto;

using EsportPlatform.API.Interfaces.services.interfaces;

using EsportPlatform.API.Interfaces.repositories.interfaces;

using EsportPlatform.API.Models;

namespace EsportPlatform.API.services;

public class HighlightService : IHighlightService
{

    private readonly IHighlightRepository _highlightRepository;

    public HighlightService(IHighlightRepository highlightRespository)
    {

        _highlightRepository = highlightRespository;
    }

    public async Task<List<HighlightDto>> GetAllHighlightsAsync()
    {
        var highlights = await _highlightRepository.GetAllAsync();
        return highlights.Select(h => new HighlightDto(h)).ToList();
    }


    public async Task<HighlightDto?> GetHighlightByIdAsync(int id)
    {
        var h = await _highlightRepository.GetByIdAsync(id);
        return h is null ? null : new HighlightDto(h);
    }

    public async Task<HighlightDto?> CreateHighlightAsync(CreateHighlightDto dto)
    {
        if (!await _highlightRepository.ExistByGameIdAsync(dto.GameId))
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

        await _highlightRepository.AddAsync(highlight);
        await _highlightRepository.SaveChangesAsync();
        return new HighlightDto(highlight);
    }

    public async Task<HighlightDto?> UpdateHighlightAsync(int id, UpdateHighlightDto dto)
    {
        var existing = await _highlightRepository.GetByIdAsync(id);
        if (existing is null) return null;
        if (!await _highlightRepository.ExistByGameIdAsync(dto.GameId)) return null;

        existing.Title = dto.Title;
        existing.GameId = dto.GameId;
        existing.MatchLabel = dto.MatchLabel;
        existing.VideoUrl = dto.VideoUrl;
        existing.Duration = dto.Duration;

        await _highlightRepository.SaveChangesAsync();
        return new HighlightDto(existing);
    }


    public async Task<bool> DeleteHighlightAsync(int id)
    {
        var highlight = await _highlightRepository.GetByIdAsync(id);
        if (highlight is null)
        {
            return false;
        }

        _highlightRepository.Delete(highlight);
        await _highlightRepository.SaveChangesAsync();
        return true;
    }
}