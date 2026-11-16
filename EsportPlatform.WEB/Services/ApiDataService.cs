using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

/// <summary>
/// Provides access to API data for Blazor pages.
/// Pages share loaded data through this scoped service.
/// </summary>
public class ApiDataService
{
    private readonly GameApiService _gameApi;
    private readonly TeamApiService _teamApi;
    private readonly TournamentApiService _tournamentApi;
    private readonly MatchApiService _matchApi;
    private readonly PlayerApiService _playerApi;
    private readonly HighlightApiService _highlightApi;
    private readonly SemaphoreSlim _loadLock = new(1, 1);
    private bool _loaded;

    public ApiDataService(
        GameApiService gameApi,
        TeamApiService teamApi,
        TournamentApiService tournamentApi,
        MatchApiService matchApi,
        PlayerApiService playerApi,
        HighlightApiService highlightApi)
    {
        _gameApi = gameApi;
        _teamApi = teamApi;
        _tournamentApi = tournamentApi;
        _matchApi = matchApi;
        _playerApi = playerApi;
        _highlightApi = highlightApi;
    }

    /// <summary>
    /// Raised when the API connection/data state changes.
    /// </summary>
    public event Action? ApiConnectionChanged;

    /// <summary>
    /// Whether the API is currently available.
    /// </summary>
    public bool IsApiAvailable { get; private set; } = true;

    // --- Game data ---

    public List<Game> Games { get; internal set; } = new();

    public int GameCount => Games.Count;

    public Game? GetGame(int id) => Games.FirstOrDefault(g => g.Id == id);

    // --- Tournament data ---

    public List<Tournament> Tournaments { get; internal set; } = new();

    public int TournamentCount => Tournaments.Count;

    public Tournament? GetTournament(int id) => Tournaments.FirstOrDefault(t => t.Id == id);

    // --- Team data ---

    public List<Team> Teams { get; internal set; } = new();

    public int TeamCount => Teams.Count;

    public Team? GetTeam(int id) => Teams.FirstOrDefault(t => t.Id == id);

    public int TeamsPendingCount => Teams.Count(t => t.Status == "Pending");

    public int TeamsApprovedCount => Teams.Count(t => t.Status == "Approved");

    public int TeamsSuspendedCount => Teams.Count(t => t.Status == "Suspended");

    /// <summary>
    /// Teams pending approval (for backward compatibility with Dashboard.razor).
    /// </summary>
    public List<Team> PendingTeams => Teams.Where(t => t.Status == "Pending").ToList();

    /// <summary>
    /// Gets players for a specific team.
    /// </summary>
    public IEnumerable<Player> PlayersForTeam(int teamId) => Players.Where(p => p.TeamId == teamId).ToList();

    // --- Player data ---

    public List<Player> Players { get; internal set; } = new();

    public int PlayerCount => Players.Count;

    // --- Match data ---

    public List<GameMatch> Matches { get; internal set; } = new();

    public int MatchCount => Matches.Count;

    public int LiveMatchesCount => Matches.Count(m => string.Equals(m.Status, "Live", StringComparison.OrdinalIgnoreCase));

    public List<GameMatch> LiveMatches => Matches.Where(m => string.Equals(m.Status, "Live", StringComparison.OrdinalIgnoreCase)).ToList();

    // --- Highlight data ---

    public List<Highlight> Highlights { get; internal set; } = new();

    // --- Convenience methods ---

    public string? GameName(int gameId) => GetGame(gameId)?.Name;

    public string? TeamName(int? teamId) => teamId.HasValue ? GetTeam(teamId.Value)?.Name : null;

    public string? TeamTag(int? teamId) => teamId.HasValue ? GetTeam(teamId.Value)?.Tag : null;

    /// <summary>
    /// Describes an error from an HttpRequestException using the API's error format.
    /// </summary>
    public static string DescribeError(HttpRequestException ex)
    {
        if (ex.StatusCode is null)
        {
            return "Cannot connect to the API server. Make sure it is running.";
        }

        return $"The API rejected the request ({(int)ex.StatusCode} {ex.StatusCode}).";
    }

    /// <summary>
    /// Marks the API as unavailable and raises the connection changed event.
    /// </summary>
    internal void MarkApiUnavailable()
    {
        if (!IsApiAvailable)
        {
            return;
        }

        IsApiAvailable = false;
        ApiConnectionChanged?.Invoke();
    }

    /// <summary>
    /// Marks the API as available and raises the connection changed event.
    /// </summary>
    internal void MarkApiAvailable()
    {
        if (IsApiAvailable)
        {
            return;
        }

        IsApiAvailable = true;
        ApiConnectionChanged?.Invoke();
    }

    /// <summary>
    /// Counts teams for a specific game.
    /// </summary>
    public int TeamCountForGame(int gameId) => Teams.Count(t => t.GameId == gameId);

    /// <summary>
    /// Counts tournaments for a specific game.
    /// </summary>
    public int TournamentCountForGame(int gameId) => Tournaments.Count(t => t.GameId == gameId);

    /// <summary>
    /// Loads shared data from the API once per scoped circuit/session.
    /// Safe to call from multiple pages; concurrent callers share one request fan-out.
    /// </summary>
    public async Task InitializeAsync(bool forceReload = false)
    {
        if (_loaded && !forceReload)
        {
            return;
        }

        await _loadLock.WaitAsync();
        try
        {
            if (_loaded && !forceReload)
            {
                return;
            }

            await LoadAllAsync();
            _loaded = true;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    public Task RefreshAsync() => InitializeAsync(forceReload: true);

    private async Task LoadAllAsync()
    {
        try
        {
            var gamesTask = _gameApi.GetGamesAsync();
            var teamsTask = _teamApi.GetTeamsAsync();
            var tournamentsTask = _tournamentApi.GetTournamentsAsync();
            var matchesTask = _matchApi.GetMatchesAsync();
            var playersTask = _playerApi.GetPlayersAsync();
            var highlightsTask = _highlightApi.GetHighlightsAsync();

            await Task.WhenAll(gamesTask, teamsTask, tournamentsTask, matchesTask, playersTask, highlightsTask);

            Games = await gamesTask ?? new List<Game>();
            Teams = await teamsTask ?? new List<Team>();
            Tournaments = await tournamentsTask ?? new List<Tournament>();
            Matches = await matchesTask ?? new List<GameMatch>();
            Players = await playersTask ?? new List<Player>();
            Highlights = await highlightsTask ?? new List<Highlight>();

            if (!IsApiAvailable)
            {
                IsApiAvailable = true;
            }

            ApiConnectionChanged?.Invoke();
        }
        catch
        {
            // Preserve existing data if any and signal unavailable state so pages can react.
            IsApiAvailable = false;
            ApiConnectionChanged?.Invoke();
        }
    }
}