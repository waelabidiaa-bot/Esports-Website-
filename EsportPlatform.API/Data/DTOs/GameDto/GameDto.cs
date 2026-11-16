using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.GameDto;

/// <summary>
/// Used for GET requests (Returning game details to the client).
/// </summary>
public record GameDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Genre { get; init; } = string.Empty;
    public string Publisher { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Platforms { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public byte[]? ImageData { get; init; }
    public string? ImageContentType { get; init; }

    public GameDto() {}

    // Entity -> DTO Constructor
    public GameDto(Game g)
    {
        Id = g.Id;
        Name = g.Name;
        Genre = g.Genre;
        Description = g.Description;
        Platforms = g.Platforms;
        IsActive = g.IsActive;
        ImageData = g.ImageData;
        ImageContentType=g.ImageContentType;
    }
}

/// <summary>
/// Used for POST requests (Creating a new game).
/// </summary>
public record CreateGameDto(
    string Name,
    string Genre,
    string Publisher,
    string Description,
    string Platforms,
    bool IsActive,
    byte[]? ImageData,
    string? ImageContentType
)
{
    public Game ToEntity() => new()
    {
        Name = Name,
        Genre = Genre,
        Publisher=Publisher,
        Description = Description,
        Platforms = Platforms,
        IsActive = IsActive,
        ImageData = ImageData,
        ImageContentType = ImageContentType
    };
}

/// <summary>
/// Used for PUT/PATCH requests (Updating an existing game).
/// </summary>
public record UpdateGameDto(
    string Name,
    string Genre,
    string Description,
    string Platforms,
    bool IsActive,
    byte[]? ImageData,
    string? ImageContentType
)
{
    public void ApplyTo(Game game)
    {
        game.Name = Name;
        game.Genre = Genre;
        game.Description = Description;
        game.Platforms = Platforms;
        game.IsActive = IsActive;
        game.ImageData = ImageData;
        game.ImageContentType = ImageContentType;
    }
}
