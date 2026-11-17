using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.TeamDto;

/// <summary>
/// A tournament a team has joined, shown on team cards and admin rosters.
/// </summary>
public record TeamTournamentDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int GameId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public decimal PrizePool { get; init; }
    public DateTime DebutDate { get; init; }
    public DateTime FinDate { get; init; }
    public DateTime JoinedAt { get; init; }

    public TeamTournamentDto() { }

    // Entity -> DTO Constructor
    public TeamTournamentDto(TournamentTeam membership)
    {
        var tournament = membership.Tournament;
        Id = tournament?.Id ?? membership.TournamentId;
        Name = tournament?.Name ?? string.Empty;
        GameId = tournament?.GameId ?? 0;
        Status = tournament?.Status ?? string.Empty;
        Format = tournament?.Format ?? string.Empty;
        PrizePool = tournament?.PrizePool ?? 0;
        DebutDate = tournament?.DebutDate ?? default;
        FinDate = tournament?.FinDate ?? default;
        JoinedAt = membership.JoinedAt;
    }
}
