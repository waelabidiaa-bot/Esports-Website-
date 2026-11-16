# EGames Jam — Esports Platform (Web Front-End)

The Blazor Web App for **EGames Jam**, an esports platform for running tournaments, managing
teams and players, publishing highlights, and following live match scores.

The app is built with **.NET 10 Blazor (Interactive Server)** and reads and writes data through
the `EsportPlatform.API` project.

## Public side

| Route | Screen |
| --- | --- |
| `/` | Home: live matches, featured tournaments, game catalog, highlights |
| `/games` | Browse games with genre filter and search |
| `/tournaments` | Tournament list with status/game filters, prize pools and slot progress |
| `/teams` | Team directory with expandable rosters |
| `/scores` | Match center: live scores, upcoming schedule, results, rankings |
| `/highlights` | Highlight reel |
| `/register` | Team registration: choose a game, captain details, roster, rules acceptance |
| `/login` · `/account/login` | Sign in with username and password (JWT) |
| `/signup` · `/account/register` | Create an account with full name, username, password and confirmation |
| `/account` | View the signed-in user and sign out |

## Admin side

| Route | Screen |
| --- | --- |
| `/admin` | Command center: KPIs, pending approvals, live matches, quick actions |
| `/admin/games` | Add / hide games in the catalog |
| `/admin/teams` | Add teams, approve or suspend rosters |
| `/admin/players` | Register players and assign them to teams |
| `/admin/tournaments` | Create tournaments with prize pool, format and dates |
| `/admin/matches` | Schedule matches, set them live, adjust scores, finalize |
| `/admin/highlights` | Publish highlights and manage the public reel |

## Project structure

```
Components/
  Layout/
    PublicLayout.razor      # public top navigation
    AdminLayout.razor       # admin sidebar shell
    NavMenu.razor           # admin navigation
  Pages/
    Home.razor
    Games.razor
    Tournaments.razor
    Teams.razor
    Scores.razor
    Highlights.razor
    Register.razor
    Login.razor
    SignUp.razor
    Account.razor
    Admin/
      Dashboard.razor
      Games.razor
      Teams.razor
      Players.razor
      Tournaments.razor
      Matches.razor
      Highlights.razor
  Shared/
    GameBanner.razor        # reusable game cover
    StatusChip.razor        # reusable status badge
Models/                     # Game, Team, Player, Tournament, GameMatch, Highlight, auth models
Services/                   # API clients, shared page data, authentication and session handling
wwwroot/app.css             # dark design system (tokens + components)
```

## Design system

- Near-black surfaces (`#08080a` / `#111114`) with soft 1px borders and layered shadows.
- Indigo brand accent (`#6d5ef8`) with cyan, green, amber and rose reserved for status.
- **Sora** for headings, **Inter** for body copy, **JetBrains Mono** for scores and data labels.
- Sentence-case headings/buttons, consistent 8–20px radii, and subtle motion only on hover/focus.
- Fully responsive: stacked public nav on mobile and a collapsing admin sidebar.
- All theme tokens live in `wwwroot/app.css` under `:root`.

## Run it

Start the API first, then the web app:

```bash
dotnet run --project ../EsportPlatform.API
dotnet run
```

Then open `http://localhost:5146`.
