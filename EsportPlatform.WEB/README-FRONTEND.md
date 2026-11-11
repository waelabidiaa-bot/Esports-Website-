# Nexus Arena — Esports Platform (Front-End Template)

A complete **front-end-only** template for an esports platform built with **.NET 10 Blazor (Interactive Server)**.
No database or API is required — all screens are powered by an in-memory mock store so the UI can be demoed
immediately and wired to a real back end later.

## Two sides

### User / public side
| Route | Screen |
| --- | --- |
| `/` | Landing page: hero, live matches, featured tournaments, game catalog, broadcasts |
| `/games` | Browse games with genre filter + search |
| `/tournaments` | Tournament list with status/game filters, prize pools and slot progress |
| `/teams` | Team directory with expandable rosters |
| `/scores` | Match center: live scores, upcoming schedule, results, power rankings |
| `/live` | Live broadcast player, live chat, on-air channels and VOD library |
| `/register` | Team registration: choose a game, captain details, roster, rules acceptance |

### Admin side
| Route | Screen |
| --- | --- |
| `/admin` | Command center: KPIs, pending approvals, live matches, quick actions |
| `/admin/games` | Add / hide games in the catalog |
| `/admin/teams` | Add teams, approve or suspend rosters |
| `/admin/players` | Register players and assign them to teams |
| `/admin/tournaments` | Create tournaments with prize pool, format and dates |
| `/admin/matches` | Schedule matches, set them live, adjust scores, finalize |
| `/admin/broadcasts` | Publish live channels / VODs and toggle their status |

## Project structure

```
Components/
  Layout/
    PublicLayout.razor      # public top navigation + footer
    AdminLayout.razor       # admin sidebar shell
    NavMenu.razor           # admin navigation
  Pages/
    Home.razor
    Games.razor
    Tournaments.razor
    Teams.razor
    Scores.razor
    Live.razor
    Register.razor
    Admin/
      Dashboard.razor
      Games.razor
      Teams.razor
      Players.razor
      Tournaments.razor
      Matches.razor
      Broadcasts.razor
  Shared/
    GameBanner.razor        # reusable gradient game cover
    StatusChip.razor        # reusable status badge
Models/                     # Game, Team, Player, Tournament, GameMatch, Broadcast
Services/MockDataService.cs # seeded in-memory data + mock mutations
wwwroot/app.css             # professional dark design system (tokens + components)
```

## Design system

The UI uses a restrained, professional dark theme:

- Near-black surfaces (`#08080a` / `#111114`) with soft 1px borders and layered shadows.
- Indigo brand accent (`#6d5ef8`) with cyan, green, amber and rose reserved for status.
- **Sora** for headings, **Inter** for body copy, **JetBrains Mono** for scores and data labels.
- Sentence-case headings/buttons, consistent 8–20px radii, and subtle motion only on hover/focus.
- Fully responsive: stacked public nav on mobile and a collapsing admin sidebar.
- All theme tokens live in `wwwroot/app.css` under `:root`, so re-branding is a one-file change.

## Run it

```bash
dotnet run
```

Then open `http://localhost:5146`.

## Connecting real data later

Replace the calls in `Services/MockDataService.cs` with your API service (the project already registers an
`HttpClient` pointing to `http://localhost:5040/`). The admin pages and registration flow call simple
methods such as `AddGame`, `AddTeam`, `AddMatch`, `RegisterTeam`, so swapping the implementation does not
require changes to the screens.

> This template is UI-only. Submitting forms mutates the in-memory demo data and resets when the app restarts.
