using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

/// <summary>
/// Local artwork shipped with the site. Used as a fallback banner for games that
/// do not have an uploaded image, and to decorate the public pages.
/// </summary>
public static class GameArt
{
    public const string ArenaScene = "/assets/images/arena-scene.jpg";
    public const string BrawlStars = "/assets/images/brawl-stars.jpg";
    public const string JinxArcane = "/assets/images/lol-jinx.png";
    public const string WildRift = "/assets/images/lol-wild-rift.jpg";
    public const string RocketLeague = "/assets/images/rocketleague.jpeg";

    private static readonly Dictionary<string, string> ByGame = new(StringComparer.OrdinalIgnoreCase)
    {
        ["League of Legends"] = JinxArcane,
        ["Rocket League"] = RocketLeague,
        ["Apex Legends"] = ArenaScene
    };

    public static string? ForGame(Game? game) =>
        game is not null && ByGame.TryGetValue(game.Name, out var art) ? art : null;
}
