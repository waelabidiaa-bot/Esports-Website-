using EsportPlatform.API.Models;

namespace EsportPlatform.API.Data.DTOs.PlayerDto;

/// <summary>
/// Used for GET requests (Returning player details to the client).
/// Contains a mapping constructor from the EF entity.
/// </summary>
public record PlayerDto
{
    public int Id { get; init; }
    public string Gamertag { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int? TeamId { get; init; }
    public string Role { get; init; } = "Flex";
    public string Country { get; init; } = "EU";
    public string Status { get; init; } = "Active";
    public string ApprovalStatus { get; init; } = "Approved";

    public PlayerDto() { }

    public PlayerDto(Player p)
    {
        Id = p.Id;
        Gamertag = p.Gamertag;
        FullName = p.FullName;
        Email = p.Email;
        TeamId = p.TeamId;
        Role = p.Role;
        Country = p.Country;
        Status = p.Status;
        ApprovalStatus = p.ApprovalStatus;
    }
}

/// <summary>
/// Used for POST requests (Creating a new player).
/// </summary>
public record CreatePlayerDto(
    string Gamertag,
    string FullName,
    string Email,
    int? TeamId,
    string Role,
    string Country,
    string Status
)
{
    public Player ToEntity() => new()
    {
        Gamertag = Gamertag,
        FullName = FullName,
        Email = Email,
        TeamId = TeamId,
        Role = Role,
        Country = Country,
        Status = Status,
        // Never trust a client-supplied approval state; services decide the final value.
        ApprovalStatus = "Pending"
    };
}


public record UpdatePlayerDto(
    string Gamertag,
    string FullName,
    string Email,
    int? TeamId,
    string Role,
    string Country,
    string Status
)
{
    public void ApplyTo(Player player)
    {
        player.Gamertag = Gamertag;
        player.FullName = FullName;
        player.Email = Email;
        player.TeamId = TeamId;
        player.Role = Role;
        player.Country = Country;
        player.Status = Status;
    }
}