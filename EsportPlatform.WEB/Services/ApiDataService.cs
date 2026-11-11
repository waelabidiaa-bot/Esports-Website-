using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EsportPlatform.WEB.Models;

namespace EsportPlatform.WEB.Services;

/// <summary>
/// Name of the HttpClient registered in Program.cs via AddHttpClient.
/// </summary>
public static class ApiHttpClient
{
    public const string Name = "Api";
}

/// <summary>
/// Server-side Blazor service that calls the EsportPlatform API over HTTP.
///
/// Security:
/// - The base address, timeout and response size cap are configured centrally in
///   Program.cs ("Api:BaseUrl", HTTPS in production) instead of being hardcoded here.
/// - Clients are resolved per call from IHttpClientFactory, so pooled handlers keep
///   rotating even though this scoped service lives as long as a circuit.
///
/// Performance:
/// - Responses are deserialized directly from the response stream (no full-string
///   buffering) and the initial load fans out all six GETs in parallel.
/// - InitializeAsync is single-flight: concurrent callers share one in-flight load.
/// - Every mutation reloads only the collection it affects instead of all six.
/// </summary>
public class ApiDataService
{
    // Web defaults already include camelCase naming and case-insensitive parsing.
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Endpoint routes, declared once so mutations and reloads cannot drift apart.
    private const string GamesUrl = "api/game";
    private const string TeamsUrl = "api/team";
    private const string PlayersUrl = "api/player";
    private const string TournamentsUrl = "api/tournament";
    private const string MatchesUrl = "api/match";
    private const string HighlightsUrl = "api/highlight";

    private readonly IHttpClientFactory _httpClientFactory;

    // Single-flight gate so concurrent callers (layout guard, retry button) share one load.
    private readonly object _initLock = new();
    private Task? _loadTask;

    // Connection state
    public bool IsApiAvailable { get; private set; }
    public bool IsApiInitialized { get; private set; }
    public string? ApiError { get; private set; }

    public event Action? ApiConnectionChanged;

    // Data collections
    public List<Game> Games { get; } = new();
    public List<Team> Teams { get; } = new();
    public List<Player> Players { get; } = new();
    public List<Tournament> Tournaments { get; } = new();
    public List<GameMatch> Matches { get; } = new();
    public List<Highlight> Highlights { get; } = new();

    public ApiDataService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // Initial API check and data loading.
    // Concurrent callers share the same in-flight load instead of duplicating requests.
    public Task InitializeAsync()
    {
        lock (_initLock)
        {
            _loadTask ??= LoadAllAsync();
            return _loadTask;
        }
    }

    private async Task LoadAllAsync()
    {
        try
        {
            try
            {
                var games = GetAsync<List<Game>>(GamesUrl);
                var teams = GetAsync<List<Team>>(TeamsUrl);
                var players = GetAsync<List<Player>>(PlayersUrl);
                var tournaments = GetAsync<List<Tournament>>(TournamentsUrl);
                var matches = GetAsync<List<GameMatch>>(MatchesUrl);
                var highlights = GetAsync<List<Highlight>>(HighlightsUrl);

                await Task.WhenAll(games, teams, players, tournaments, matches, highlights);

                Replace(Games, await games ?? new());
                Replace(Teams, await teams ?? new());
                Replace(Players, await players ?? new());
                Replace(Tournaments, await tournaments ?? new());
                Replace(Matches, await matches ?? new());
                Replace(Highlights, await highlights ?? new());

                SetApiAvailable();
            }
            catch (HttpRequestException ex)
            {
                if (IsConnectionFailure(ex))
                {
                    SetApiUnavailable("Cannot connect to the API server. Make sure the API is running.");
                }
                else
                {
                    SetApiAvailable();
                    ApiError = $"The API responded, but a request failed: {ex.Message}";
                }
            }
            catch (TaskCanceledException)
            {
                SetApiUnavailable("The API server did not respond in time.");
            }
        }
        finally
        {
            IsApiInitialized = true;

            // Allow the next InitializeAsync call to trigger a fresh load (retry).
            lock (_initLock)
            {
                _loadTask = null;
            }
        }
    }

    // Lookup helpers (unchanged)
    public Game? GetGame(int id) => Games.FirstOrDefault(g => g.Id == id);
    public Team? GetTeam(int? id) => Teams.FirstOrDefault(t => t.Id == id);
    public Tournament? GetTournament(int id) => Tournaments.FirstOrDefault(t => t.Id == id);
    public string GameName(int id) => GetGame(id)?.Name ?? "Unknown Game";
    public string TeamName(int? id) => GetTeam(id)?.Name ?? "TBD";
    public string TeamTag(int? id) => GetTeam(id)?.Tag ?? "TBD";
    public int TeamCountForGame(int gameId) => Teams.Count(t => t.GameId == gameId);
    public int TournamentCountForGame(int gameId) => Tournaments.Count(t => t.GameId == gameId);
    public IEnumerable<Player> PlayersForTeam(int teamId) => Players.Where(p => p.TeamId == teamId);
    public IEnumerable<GameMatch> MatchesForTeam(int teamId) => Matches.Where(m => m.TeamAId == teamId || m.TeamBId == teamId);
    public IEnumerable<GameMatch> LiveMatches => Matches.Where(m => m.Status == "Live");
    public IEnumerable<Team> PendingTeams => Teams.Where(t => t.Status == "Pending");

    // Asynchronous Mutations
    public async Task<Game> AddGameAsync(Game game)
    {
        var created = await SendAsync<Game>(HttpMethod.Post, GamesUrl, game);
        await ReloadAsync(GamesUrl);
        return created!;
    }

    public async Task<Game> UpdateGameAsync(Game game)
    {
        var updated = await SendAsync<Game>(HttpMethod.Put, $"{GamesUrl}/{game.Id}", game);
        await ReloadAsync(GamesUrl);
        return updated!;
    }

    public async Task ToggleGameAsync(int id)
    {
        await SendAsync<Game>(HttpMethod.Patch, $"{GamesUrl}/{id}/toggle");
        await ReloadAsync(GamesUrl);
    }

    public async Task<Team> AddTeamAsync(Team team)
    {
        var created = await SendAsync<Team>(HttpMethod.Post, TeamsUrl, team);
        await ReloadAsync(TeamsUrl);
        return created!;
    }

    public async Task AddPlayerAsync(Player player)
    {
        await SendAsync<Player>(HttpMethod.Post, PlayersUrl, player);
        await ReloadAsync(PlayersUrl);
    }

    public async Task<Tournament> AddTournamentAsync(Tournament tournament)
    {
        var created = await SendAsync<Tournament>(HttpMethod.Post, TournamentsUrl, tournament);
        await ReloadAsync(TournamentsUrl);
        return created!;
    }

    public async Task<GameMatch> AddMatchAsync(GameMatch match)
    {
        var created = await SendAsync<GameMatch>(HttpMethod.Post, MatchesUrl, match);
        await ReloadAsync(MatchesUrl);
        return created!;
    }

    public async Task AddHighlightAsync(Highlight highlight)
    {
        await SendAsync<Highlight>(HttpMethod.Post, HighlightsUrl, highlight);
        await ReloadAsync(HighlightsUrl);
    }

    public async Task DeleteHighlightAsync(int id)
    {
        await SendAsync<Highlight>(HttpMethod.Delete, $"{HighlightsUrl}/{id}");
        await ReloadAsync(HighlightsUrl);
    }

    public async Task RegisterTeamAsync(Team team, IEnumerable<Player> roster)
    {
        await SendAsync<Team>(
            HttpMethod.Post,
            $"{TeamsUrl}/register",
            new
            {
                team,
                roster = roster.ToList()
            });

        // Registration touches both collections, so both are refreshed in parallel.
        await Task.WhenAll(ReloadAsync(TeamsUrl), ReloadAsync(PlayersUrl));
    }

    public async Task ApproveTeamAsync(int id) => await SetTeamStatusAsync(id, "Approved");
    public async Task SuspendTeamAsync(int id) => await SetTeamStatusAsync(id, "Suspended");

    private async Task SetTeamStatusAsync(int id, string status)
    {
        await SendAsync<Team>(HttpMethod.Patch, $"{TeamsUrl}/{id}/status", new { status });
        await ReloadAsync(TeamsUrl);
    }

    public async Task SetMatchLiveAsync(int id) => await SetMatchStatusAsync(id, "Live");
    public async Task FinalizeMatchAsync(int id) => await SetMatchStatusAsync(id, "Completed");

    private async Task SetMatchStatusAsync(int id, string status)
    {
        await SendAsync<GameMatch>(HttpMethod.Patch, $"{MatchesUrl}/{id}/status", new { status });
        await ReloadAsync(MatchesUrl);
    }

    public async Task AdjustScoreAsync(int id, bool teamA)
    {
        var match = Matches.FirstOrDefault(m => m.Id == id);
        if (match is null) return;

        var scoreA = match.ScoreA + (teamA ? 1 : 0);
        var scoreB = match.ScoreB + (teamA ? 0 : 1);

        await SendAsync<GameMatch>(HttpMethod.Patch, $"{MatchesUrl}/{id}/score", new { scoreA, scoreB });
        await ReloadAsync(MatchesUrl);
    }

    public Task CheckConnection() => InitializeAsync();

    /// <summary>
    /// Human-readable text for an API failure that is not a connection problem,
    /// e.g. a 409 Conflict when a team name is already taken.
    /// </summary>
    public static string DescribeError(HttpRequestException ex) =>
        ex.StatusCode is { } status
            ? $"The API rejected the operation ({(int)status} {status})."
            : ex.Message;

    private void SetApiUnavailable(string message)
    {
        bool changed = IsApiAvailable;
        IsApiAvailable = false;
        ApiError = message;

        if (changed)
            ApiConnectionChanged?.Invoke();
    }

    private void SetApiAvailable()
    {
        bool changed = !IsApiAvailable;
        IsApiAvailable = true;
        ApiError = null;

        if (changed)
            ApiConnectionChanged?.Invoke();
    }

    private static bool IsConnectionFailure(HttpRequestException ex)
    {
        return ex.StatusCode is null || (int)ex.StatusCode >= 500;
    }

    // Reloads a single collection instead of all six after a mutation.
    // Reload failures do not rethrow: connection problems surface through the
    // ApiConnectionChanged event, which the layout guard uses to redirect.
    private async Task ReloadAsync(string url)
    {
        try
        {
            switch (url)
            {
                case GamesUrl:
                    Replace(Games, await GetAsync<List<Game>>(url) ?? new());
                    break;
                case TeamsUrl:
                    Replace(Teams, await GetAsync<List<Team>>(url) ?? new());
                    break;
                case PlayersUrl:
                    Replace(Players, await GetAsync<List<Player>>(url) ?? new());
                    break;
                case TournamentsUrl:
                    Replace(Tournaments, await GetAsync<List<Tournament>>(url) ?? new());
                    break;
                case MatchesUrl:
                    Replace(Matches, await GetAsync<List<GameMatch>>(url) ?? new());
                    break;
                case HighlightsUrl:
                    Replace(Highlights, await GetAsync<List<Highlight>>(url) ?? new());
                    break;
            }

            SetApiAvailable();
        }
        catch (HttpRequestException ex) when (IsConnectionFailure(ex))
        {
            SetApiUnavailable("Cannot connect to the API server.");
        }
        catch (TaskCanceledException)
        {
            SetApiUnavailable("The API server timed out.");
        }
    }

    private static void Replace<T>(List<T> target, IEnumerable<T> source)
    {
        target.Clear();
        target.AddRange(source);
    }

    // Deserializes straight from the response stream (no intermediate string).
    // GetFromJsonAsync throws HttpRequestException for non-success status codes.
    private async Task<T?> GetAsync<T>(string url)
    {
        var http = _httpClientFactory.CreateClient(ApiHttpClient.Name);
        return await http.GetFromJsonAsync<T>(url, JsonOptions);
    }

    private async Task<T?> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        var http = _httpClientFactory.CreateClient(ApiHttpClient.Name);

        try
        {
            using var request = new HttpRequestMessage(method, url);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body, options: JsonOptions);
            }

            using var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            if (response.StatusCode == HttpStatusCode.NoContent ||
                response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

            return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        }
        catch (HttpRequestException ex) when (IsConnectionFailure(ex))
        {
            SetApiUnavailable("The API server is unreachable or experiencing an error.");
            throw;
        }
        catch (TaskCanceledException)
        {
            SetApiUnavailable("The API server did not respond in time.");
            throw;
        }
    }
}
