namespace EsportPlatform.WEB.Models;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tag { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string Region { get; set; } = "EU";
    public string Captain { get; set; } = string.Empty;
    public int RosterSize { get; set; }
    public int MaxRoster { get; set; } = 5;
    public int Points { get; set; }
    public string Status { get; set; } = "Approved";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
