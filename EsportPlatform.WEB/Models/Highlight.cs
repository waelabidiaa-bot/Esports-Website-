namespace EsportPlatform.WEB.Models;

public class Highlight
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int GameId { get; set; }
    public string MatchLabel { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string Duration { get; set; } = "10:00";
    public int Views { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
}
