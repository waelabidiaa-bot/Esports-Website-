



namespace EsportPlatform.API.Data.DTOs.GameDto;

public record UpdateGameDto(
    string Name,
    string Genre,
    string Description,
    string Platforms,
    bool IsActive,
    byte[]? ImageData
);   