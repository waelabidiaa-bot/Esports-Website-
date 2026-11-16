namespace EsportPlatform.WEB.Models;

public class Match
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public int GameId { get; set; }
    public int? TeamAId { get; set; }
    public int? TeamBId { get; set; }
    public int ScoreA { get; set; }
    public int ScoreB { get; set; }
    public string Stage { get; set; } = "Group Stage";
    public string BestOf { get; set; } = "Bo3";
    public string Status { get; set; } = "Upcoming";
    public DateTime? ScheduledAt { get; set; }
}