using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.TournamentDto;

/// <summary>
/// Used for GET requests (Returning tournament details to the client).
/// </summary>
public record TournamentDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime DebutDate { get; init; }
    public DateTime FinDate { get; init; }
    public int GameId { get; init; }
    public string Format { get; init; } = "Single Elimination";
    public decimal PrizePool { get; init; }
    public int MaxTeams { get; init; } = 16;
    public int RegisteredTeams { get; init; }
    public string Status { get; init; } = "Upcoming";
    public string Description { get; init; } = string.Empty;
    public byte[]? ImageData { get; init; }
    public string? ImageContentType { get; init; }

    public TournamentDto() {}

    // Entity -> DTO Constructor
    public TournamentDto(Tournament t)
    {
        Id = t.Id;
        Name = t.Name;
        DebutDate = t.DebutDate;
        FinDate = t.FinDate;
        GameId = t.GameId;
        Format = t.Format;
        PrizePool = t.PrizePool;
        MaxTeams = t.MaxTeams;
        RegisteredTeams = t.RegisteredTeams;
        Status = t.Status;
        Description = t.Description;
        ImageData = t.ImageData;
        ImageContentType = t.ImageContentType;
    }
}

/// <summary>
/// Used for POST requests (Creating a new tournament).
/// </summary>
public record CreateTournamentDto(
    string Name,
    DateTime DebutDate,
    DateTime FinDate,
    int GameId,
    string Format,
    decimal PrizePool,
    int MaxTeams,
    string Description,
    byte[]? ImageData,
    string? ImageContentType
)
{
    public Tournament ToEntity() => new()
    {
        Name = Name,
        DebutDate = DebutDate,
        FinDate = FinDate,
        GameId = GameId,
        Format = Format,
        PrizePool = PrizePool,
        MaxTeams = MaxTeams,
        RegisteredTeams = 0,
        Status = "Upcoming",
        Description = Description,
        ImageData = ImageData,
        ImageContentType = ImageContentType
    };
}

/// <summary>
/// Used for PUT/PATCH requests (Updating an existing tournament).
/// </summary>
public record UpdateTournamentDto(
    string Name,
    DateTime DebutDate,
    DateTime FinDate,
    int GameId,
    string Format,
    decimal PrizePool,
    int MaxTeams,
    string Status,
    string Description,
    byte[]? ImageData,
    string? ImageContentType
)
{
    public void ApplyTo(Tournament tournament)
    {
        tournament.Name = Name;
        tournament.DebutDate = DebutDate;
        tournament.FinDate = FinDate;
        tournament.GameId = GameId;
        tournament.Format = Format;
        tournament.PrizePool = PrizePool;
        tournament.MaxTeams = MaxTeams;
        tournament.Status = Status;
        tournament.Description = Description;
        tournament.ImageData = ImageData;
        tournament.ImageContentType = ImageContentType;
    }
}