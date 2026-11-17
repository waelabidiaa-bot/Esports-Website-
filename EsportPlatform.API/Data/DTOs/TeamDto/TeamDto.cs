using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.TeamDto;

/// <summary>
/// Used for GET requests (Returning team details to the client).
/// </summary>
public record TeamDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Tag { get; init; } = string.Empty;
    public int GameId { get; init; }
    public string Region { get; init; } = "EU";
    public string Captain { get; init; } = string.Empty;
    public int RosterSize { get; init; }
    public int MaxRoster { get; init; } = 5;
    public int Points { get; init; }
    public string Status { get; init; } = "Pending";
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Tournaments this team has joined, ordered by their start date.
    /// </summary>
    public List<TeamTournamentDto> Tournaments { get; init; } = new();

    public TeamDto() { }

    // Entity -> DTO Constructor
    public TeamDto(Team t)
    {
        Id = t.Id;
        Name = t.Name;
        Tag = t.Tag;
        GameId = t.GameId;
        Region = t.Region;
        Captain = t.Captain;
        RosterSize = t.RosterSize;
        MaxRoster = t.MaxRoster;
        Points = t.Points;
        Status = t.Status;
        CreatedAt = t.CreatedAt;

        Tournaments = t.TournamentTeams
            .Where(tt => tt.Tournament is not null)
            .OrderBy(tt => tt.Tournament.DebutDate)
            .ThenBy(tt => tt.Tournament.Name)
            .Select(tt => new TeamTournamentDto(tt))
            .ToList();
    }
}

/// <summary>
/// Used for POST requests (Creating a new team).
/// </summary>
public record CreateTeamDto(
    string Name,
    string Tag,
    int GameId,
    string Region,
    string Captain,
    int MaxRoster
)
{
    public Team ToEntity() => new()
    {
        Name = Name,
        Tag = Tag,
        GameId = GameId,
        Region = Region,
        Captain = Captain,
        MaxRoster = MaxRoster,
        Status = "Pending",
        Points = 0,
        RosterSize = 0,
        CreatedAt = DateTime.UtcNow
    };
}

/// <summary>
/// Used for PUT/PATCH requests (Updating an existing team).
/// </summary>
public record UpdateTeamDto(
    string Name,
    string Tag,
    int GameId,
    string Region,
    string Captain,
    int MaxRoster,
    int Points,
    string Status
)
{
    public void ApplyTo(Team team)
    {
        team.Name = Name;
        team.Tag = Tag;
        team.GameId = GameId;
        team.Region = Region;
        team.Captain = Captain;
        team.MaxRoster = MaxRoster;
        team.Points = Points;
        team.Status = Status;
    }
}