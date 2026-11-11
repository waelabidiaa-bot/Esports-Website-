namespace EsportPlatform.API.Models;

public class Tournament
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DebutDate { get; set; }
    public DateTime FinDate { get; set; }

    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public string Format { get; set; } = "Single Elimination";
    public decimal PrizePool { get; set; }
    public int MaxTeams { get; set; } = 16;
    public int RegisteredTeams { get; set; }
    public string Status { get; set; } = "Upcoming";
    public string Description { get; set; } = string.Empty;

    public byte[]? ImageData { get; set; }
    public string? ImageContentType { get; set; }
}
