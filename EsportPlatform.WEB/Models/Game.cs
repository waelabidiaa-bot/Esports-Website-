namespace EsportPlatform.WEB.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Platforms { get; set; } = "PC";
    public string Accent { get; set; } = "#6d5ef8";
    public bool IsActive { get; set; } = true;
    public byte[]? ImageData { get; set; }
    public string? ImageContentType { get; set; }
}
