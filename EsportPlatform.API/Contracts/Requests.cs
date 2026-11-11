using EsportPlatform.API.Models;

namespace EsportPlatform.API.Contracts;

public class StatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class TeamRegistrationRequest
{
    public Team Team { get; set; } = new();
    public List<Player> Roster { get; set; } = new();
}

public class ScoreRequest
{
    public int ScoreA { get; set; }
    public int ScoreB { get; set; }
}
