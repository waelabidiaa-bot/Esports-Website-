namespace EsportPlatform.WEB.Models;

public class TeamTournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public decimal PrizePool { get; set; }
    public DateTime DebutDate { get; set; }
    public DateTime FinDate { get; set; }
    public DateTime JoinedAt { get; set; }
}
