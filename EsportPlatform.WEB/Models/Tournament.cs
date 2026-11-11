namespace EsportPlatform.WEB.Models;

using System.Text.Json.Serialization;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string Format { get; set; } = "Single Elimination";
    public decimal PrizePool { get; set; }
    public int MaxTeams { get; set; } = 16;
    public int RegisteredTeams { get; set; }

    [JsonPropertyName("debutDate")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [JsonPropertyName("finDate")]
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
    public string Status { get; set; } = "Upcoming";
    public string Description { get; set; } = string.Empty;
}
