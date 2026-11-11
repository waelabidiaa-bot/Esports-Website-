

namespace EsportPlatform.API.Data.DTOs.HighlightDto;

public record UpdateHighlightDto(
    string Title,
    int GameId,
    string MatchLabel,
    string VideoUrl,
    string Duration
);   