using System.Net;
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
    public IEnumerable<Player> PlayersForTeam(int teamId) => Players
        .Where(p => p.TeamId == teamId && p.ApprovalStatus == "Approved")
        .ToList();

    /// <summary>
    /// Number of players for a given game.
    /// </summary>
    public int PlayerCountForGame(int gameId) => Players.Count(p => p.GameId == gameId);

    // --- Player data ---

    public List<Player> Players { get; internal set; } = new();

    public int PlayerCount => Players.Count;

    public List<Player> PendingPlayers => Players
        .Where(p => p.ApprovalStatus == "Pending")
        .ToList();

    public List<Player> ApprovedPlayers => Players
        .Where(p => p.ApprovalStatus == "Approved")
        .ToList();

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
    /// Turns an exception into a short, user-facing explanation.
    /// </summary>
    /// <param name="ex">The exception that was caught.</param>
    /// <param name="action">What the user was trying to do, for example "load the game catalog".</param>
    public static string DescribeError(Exception ex, string action = "process your request")
    {
        var task = string.IsNullOrWhiteSpace(action) ? "process your request" : action;

        return ex switch
        {
            HttpRequestException { StatusCode: null } =>
                $"We couldn't reach the EGames Jam server while trying to {task}. Check your connection and try again.",

            HttpRequestException { StatusCode: HttpStatusCode.Unauthorized } =>
                $"Your session has expired while trying to {task}. Please sign in again.",

            HttpRequestException { StatusCode: HttpStatusCode.Forbidden } =>
                $"You don't have permission to {task}.",

            HttpRequestException { StatusCode: HttpStatusCode.NotFound } =>
                $"The requested item could not be found while trying to {task}.",

            HttpRequestException { StatusCode: HttpStatusCode.Conflict } =>
                "This item is still referenced by other records. Remove or reassign those records first.",

            HttpRequestException { StatusCode: HttpStatusCode.RequestEntityTooLarge } =>
                "That file is too large to upload. Choose a smaller image and try again.",

            HttpRequestException =>
                $"The server couldn't complete the request to {task}. Please try again.",

            TaskCanceledException =>
                $"The request to {task} took too long. Please try again.",

            IOException =>
                "We couldn't read the uploaded file. Please try again.",

            _ =>
                $"Something went wrong while trying to {task}. Please try again."
        };
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