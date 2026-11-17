namespace EsportPlatform.API.Models;

/// <summary>
/// Join entity recording that a team has joined a tournament.
/// A team can join the same tournament only once.
/// </summary>
public class TournamentTeam
{
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
