using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.HighlightDto;

public record HighlightDto
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string MatchLabel { get; init; } = string.Empty;
    public string VideoUrl { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public int Views { get; init; }
    public DateTime PublishedAt { get; init; }
    public int GameId { get; init; }

    public HighlightDto() { }

    public HighlightDto(Highlight h)
    {
        Id = h.Id;
        Title = h.Title;
        MatchLabel = h.MatchLabel;
        VideoUrl = h.VideoUrl;
        Duration = h.Duration;
        Views = h.Views;
        PublishedAt = h.PublishedAt;
        GameId = h.GameId;
    }
}   