namespace EsportPlatform.API.Data.DTOs.HighlightDto;








public record CreateHighlightDto(
                                string Title,
                                int GameId,
                                string  MatchLabel,
                                string VideoUrl,
                                string Duration,
                                int Views

);
