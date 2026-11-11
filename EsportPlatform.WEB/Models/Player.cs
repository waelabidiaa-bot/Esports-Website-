namespace EsportPlatform.WEB.Models;

public class Player
{
    public int Id { get; set; }
    public string Gamertag { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? TeamId { get; set; }
    public string Role { get; set; } = "Flex";
    public string Country { get; set; } = "EU";
    public string Status { get; set; } = "Active";
}
