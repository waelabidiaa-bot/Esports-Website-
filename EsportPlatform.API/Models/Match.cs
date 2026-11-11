namespace EsportPlatform.API.Models;

public class Match
{
    public int Id { get; set; }
    public int TournamentId { get; set; }
    public Tournament Tournament { get; set; } = null!;

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public int? TeamAId { get; set; }
    public Team? TeamA { get; set; }

    public int? TeamBId { get; set; }
    public Team? TeamB { get; set; }

    public int ScoreA { get; set; }
    public int ScoreB { get; set; }

    public string Stage { get; set; } = "Group Stage";
    public string BestOf { get; set; } = "Bo3";
    public string Status { get; set; } = "Upcoming";
    public DateTime MatchDate { get; set; }

   
}
