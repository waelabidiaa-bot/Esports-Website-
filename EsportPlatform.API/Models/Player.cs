namespace EsportPlatform.API.Models;

public class Player
{
    public int Id { get; set; }
    public string Gamertag { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    public string Role { get; set; } = "Flex";
    public string Country { get; set; } = "EU";
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Approval state of this player's roster membership: Pending, Approved or Rejected.
    /// Players added by users start as Pending and only count towards the effective
    /// roster after an administrator approves them.
    /// </summary>
    public string ApprovalStatus { get; set; } = "Approved";
}
