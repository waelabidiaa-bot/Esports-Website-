# EGames Jam — Esports Platform

A full-stack esports platform for running tournaments, managing teams and players, publishing
highlights, and following live match scores. Visitors browse the public catalog and match center;
signed-in users can register a team and roster or sign up for an account; administrators manage
every part of the platform from a dedicated back office.

The repository is a .NET 10 solution made up of three projects: a layered REST API, a Blazor
interactive-server web app, and an xUnit/Playwright test project.

---

## Contents

- [Solution at a glance](#solution-at-a-glance)
- [Features](#features)
- [Technology stack](#technology-stack)
- [Architecture](#architecture)
- [Domain model and business rules](#domain-model-and-business-rules)
- [API reference](#api-reference)
- [Authentication and authorization](#authentication-and-authorization)
- [Frontend](#frontend)
- [Getting started](#getting-started)
- [Configuration](#configuration)
- [Database, migrations and seed data](#database-migrations-and-seed-data)
- [Testing](#testing)
- [Project structure](#project-structure)
- [Roadmap, known gaps and production notes](#roadmap-known-gaps-and-production-notes)

---

## Solution at a glance

| Project | Type | Role | Local URL |
| --- | --- | --- | --- |
| `EsportPlatform.API` | ASP.NET Core Web API (`.NET 10`) | REST backend, identity, business logic, EF Core/SQLite persistence | `http://localhost:5040` (`https://localhost:7254`) |
| `EsportPlatform.WEB` | Blazor Web App, Interactive Server (`.NET 10`) | Public site + admin back office; calls the API over HTTP | `http://localhost:5146` (`https://localhost:7026`) |
| `EsportPlatform.Tests` | xUnit test library (`.NET 10`) | Controller/unit tests (EF Core InMemory) and Playwright end-to-end smoke tests | — |

There is no `.sln` file: each project is run directly with `dotnet run --project <path>`.
`EsportPlatform.WEB` references the API project so it can share DTO and contract types, but it
does **not** use the database directly — all data flows through the REST API.

---

## Features

### Public site

| Route | What it does |
| --- | --- |
| `/` | Home dashboard: live matches, featured tournaments, game catalog, highlight reel |
| `/games` | Browse games with genre filtering and search |
| `/tournaments` | Tournament list with status/game filters, prize pools and registration progress |
| `/teams` | Team directory with expandable rosters and tournament history |
| `/players` | Player directory |
| `/scores` | Match center: live scores, upcoming schedule, results, rankings |
| `/highlights` | Highlight reel with video links, duration and view counts |
| `/register` | Team registration wizard: game, captain details, roster, rules acceptance |
| `/login`, `/account/login` | Sign in with username and password (JWT) |
| `/signup`, `/account/register` | Create an account (full name, username, password, confirmation) |
| `/account` | View the signed-in user and sign out |

### Admin back office (`/admin`)

| Route | What it does |
| --- | --- |
| `/admin` | Command center: KPIs, pending approvals, live matches, quick actions |
| `/admin/games` | Add games, edit catalog details, hide/show games in the catalog |
| `/admin/teams` | Add teams, approve or suspend rosters |
| `/admin/players` | Register players and assign them to teams |
| `/admin/tournaments` | Create tournaments with prize pool, format and dates |
| `/admin/matches` | Schedule matches, set them live, adjust scores, finalize results |
| `/admin/highlights` | Publish highlights and manage the public reel |

Supporting routes: `/not-found`, `/Error`, `/server-error`.

---

## Technology stack

**Backend (`EsportPlatform.API`)**

- ASP.NET Core 10 Web API, minimal hosting in `Program.cs`
- Entity Framework Core 10 with SQLite (`Microsoft.EntityFrameworkCore.Sqlite`)
- ASP.NET Core Identity (`IdentityDbContext<ApplicationUser>`) + JWT bearer authentication
- Layered architecture: controllers → services → repositories → EF Core `AppDbContext`
- DTOs per resource for requests/responses (`Create*`, `Update*`, `*` read models)
- Generic `IRepository<T>` / `Repository<T>` plus domain-specific repositories and interfaces
- Shared `PaginationQuery` / `PagedResult<T>` contract
- CORS policy `AllowBlazorClient` for the Blazor front end
- EF Core migrations applied automatically at startup

**Frontend (`EsportPlatform.WEB`)**

- .NET 10 Blazor Web App with Interactive Server render mode and static assets
- Named `HttpClient` (`ApiHttpClient.Name`) configured from `Api:BaseUrl`
- Scoped `ApiClient` (GET/paginated GET/send with bearer token), `ApiDataService` (shared cache),
  per-resource API services, `AuthService` and `JwtAuthenticationStateProvider`
- `ProtectedSessionStorage` for the JWT session; data-protection keys persisted under
  `.aspnet-data-protection-keys-web/`
- Bootstrap + MudBlazor packages, a custom dark design system in `wwwroot/app.css`,
  Google Fonts (Sora, Inter, JetBrains Mono, Orbitron/Chakra Petch/Black Ops One) and
  Bootstrap Icons



---

## Architecture

```
Browser
   │
   ▼
Blazor Interactive Server (EsportPlatform.WEB)
   Components/Pages ──▶ Services/*ApiService ──▶ ApiClient ──▶ named HttpClient
                                                    │                │
                             AuthService (JWT in ProtectedSessionStorage)
                                                    │                │
                                                    └────────────────┘
                                                     HTTP + Bearer token
                                                            ▼
                                             ASP.NET Core API (EsportPlatform.API)
                                             Controllers (routing, status codes)
                                                     ▼
                                             Services (business rules)
                                                     ▼
                                             Repositories (queries, EF Core)
                                                     ▼
                                             AppDbContext ──▶ SQLite (esports.db)
```

- **Controllers** are thin: they map HTTP to service calls, enforce `[Authorize]` roles and
  translate `null`/`false` results into `400/404/409` responses.
- **Services** own validation and invariants (uniqueness, roster capacity, tournament
  membership, match scheduling). They depend only on repository interfaces, which keeps them
  mockable.
- **Repositories** encapsulate EF Core queries (paging, `Include`, existence checks) and
  `SaveChangesAsync`.
- **DTOs** decouple EF entities from the public API; entities are never returned directly by
  the production controllers.
- **Dependency injection**: all services and repositories are registered as scoped in
  `Program.cs`; `JwtSettings`, Identity managers, `RoleManager<IdentityRole>`, the DbContext,
  CORS, authentication and authorization are configured in the same file.
- JSON is configured with `ReferenceHandler.IgnoreCycles` to avoid cyclic navigation warnings.

---

## Domain model and business rules

### Entities

| Entity | Purpose |
| --- | --- |
| `Game` | Game catalog entry (name, genre, publisher, platforms, accent color, active flag, cover image). Game names are unique. |
| `Team` | Team with tag, game, region, captain, roster size/cap, points, status, creation date. |
| `Player` | Gamertag, full name, email, optional team, role, country, status. |
| `Tournament` | Name, dates, game, format, prize pool, team cap/registrations, status, description, cover image. |
| `Match` | Tournament + game + two optional teams, scores, stage, best-of format, status, date. |
| `Highlight` | Title, game, match label, video URL, duration, views, publish date. |
| `TournamentTeam` | Join entity (composite key) recording that a team joined a tournament. |
| `ApplicationUser` | ASP.NET Core Identity user with an extra `FullName` property. |

Relationships and delete behaviors are configured in `AppDbContext.OnModelCreating`: games,
tournaments, matches and highlights use restrictive FKs where appropriate; deleting a tournament
cascades its matches and memberships; deleting a team sets `Match.TeamA/TeamB` and `Player.TeamId`
to `null`.

### Status values and validation

- **Team status**: `Approved`, `Pending`, `Suspended` (`PATCH /api/team/{id}/status`).
- **Player status**: `Active`, `Bench`, `Inactive`.
- **Match status**: `Upcoming`, `Live`, `Completed`.
- **Best-of formats**: `Bo1`, `Bo3`, `Bo5`, `Bo7`.
- **Tournament status**: free text in DTOs; values used by seed data and scheduling rules include
  `Upcoming`, `Open`, `Live`, `Completed`. Completed/finished/cancelled/archived tournaments
  reject new or edited matches.
- **Duplicate checks**: unique game name, unique tournament name, unique team name/tag, unique
  player gamertag; duplicates produce `409 Conflict` or `400 Bad Request` depending on the route.
- **Roster rules**: a player cannot be added when `RosterSize >= MaxRoster`; creating/updating/
  deleting a player adjusts the old and new team's `RosterSize` accordingly.
- **Team registration** (`POST /api/team/register`): creates a `Pending` team plus its roster,
  finds an eligible tournament for the game, increments `RegisteredTeams`, and writes a
  `TournamentTeam` membership.
- **Match scheduling** (`MatchService`): the tournament must exist, both teams must be approved
  and play the tournament's game, the match date must fall inside tournament dates, a team cannot
  play itself, a pairing can appear only once per stage, and no team can be in two matches at the
  same time. Upcoming matches may have no teams yet (TBD slot).
- **Score updates**: negative scores are rejected; setting a match live requires both teams.
- **Auth DTO validation**: DataAnnotations enforce full name, username pattern, password length
  and confirmation; registration always assigns the `User` role (never an elevated role).

### Pagination

Every `GET` collection endpoint accepts `?pageNumber=1&pageSize=10` (`PageSize` is clamped to
`1..100`, default 10). Responses are `PagedResult<T>` objects:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 0,
  "totalPages": 0,
  "hasPrevious": false,
  "hasNext": false
}
```

The web app's `ApiClient.GetAllAsync<T>` walks all pages at the maximum page size so pages can
work with complete lists.

---

## API reference

Base URL: `http://localhost:5040`. Root `GET /` returns a health payload with the API name,
`status: ok`, and the list of resources.

Legend: **Anon** = `[AllowAnonymous]`, **User** = `User`/`Admin`/`Player` roles,
**Admin** = `[Authorize(Roles = "Admin")]`, **Auth** = any authenticated user.

| Resource | Method & path | Access | Description |
| --- | --- | --- | --- |
| Auth | `POST /api/auth/register` | Anon | Register (`RegisterDto`), returns `AuthResponseDto` with JWT |
| Auth | `POST /api/auth/login` | Anon | Login (`LoginDto`), returns JWT |
| Auth | `GET /api/auth/me` | Auth | Return current user id, name, full name and roles |
| Game | `GET /api/game` | Anon | List active games (paged) |
| Game | `GET /api/game/{id}` | Anon | Get one game |
| Game | `POST /api/game` | Admin | Create game; `409` on duplicate name |
| Game | `PUT /api/game/{id}` | Admin | Update game |
| Game | `PATCH /api/game/{id}/toggle` | Admin | Toggle `IsActive` |
| Team | `GET /api/team` | Anon | List teams with tournament memberships (paged) |
| Team | `GET /api/team/{id}` | Anon | Get one team |
| Team | `POST /api/team` | User | Create team (`Pending`) |
| Team | `PUT /api/team/{id}` | Admin | Update team |
| Team | `DELETE /api/team/{id}` | Admin | Delete team (`204`) |
| Team | `PATCH /api/team/{id}/status` | Admin | Set `Approved`/`Pending`/`Suspended` |
| Team | `POST /api/team/register` | User | Register team + roster and reserve a tournament slot |
| Team | `POST /api/team/{id}/players` | User | Add a player to a team |
| Tournament | `GET /api/tournament` | Anon | List tournaments (paged) |
| Tournament | `GET /api/tournament/{id}` | Anon | Get one tournament |
| Tournament | `POST /api/tournament` | Admin | Create tournament |
| Tournament | `PUT /api/tournament/{id}` | Admin | Update tournament |
| Tournament | `DELETE /api/tournament/{id}` | Admin | Delete tournament (`204`) |
| Player | `GET /api/player` | Anon | List players (paged) |
| Player | `GET /api/player/{id}` | Anon | Get one player |
| Player | `POST /api/player` | User | Create player profile |
| Player | `PUT /api/player/{id}` | Admin | Update player |
| Player | `DELETE /api/player/{id}` | Admin | Delete player (`204`) |
| Player | `PATCH /api/player/{id}/status` | Admin | Set `Active`/`Bench`/`Inactive` |
| Match | `GET /api/match` | Anon | List matches (paged) |
| Match | `GET /api/match/{id}` | Anon | Get one match |
| Match | `POST /api/match` | Admin | Schedule match (full validation, see above) |
| Match | `PUT /api/match/{id}` | Admin | Update match (same validation) |
| Match | `DELETE /api/match/{id}` | Admin | Delete match (`204`) |
| Match | `PATCH /api/match/{id}/status` | Admin | Set `Upcoming`/`Live`/`Completed` |
| Match | `PATCH /api/match/{id}/score` | Admin | Update scores (`UpdateScoreDto`) |
| Highlight | `GET /api/highlight` | Anon | List highlights (paged) |
| Highlight | `GET /api/highlight/{id}` | Anon | Get one highlight |
| Highlight | `POST /api/highlight` | Admin | Publish highlight |
| Highlight | `PUT /api/highlight/{id}` | Admin | Update highlight |
| Highlight | `DELETE /api/highlight/{id}` | Admin | Delete highlight (`204`) |

`EsportPlatform.API.http` contains a starter HTTP request file for manual API calls.

---

## Authentication and authorization

- ASP.NET Core Identity stores users/roles in the same SQLite database through `AppDbContext`.
  `ApplicationUser` adds `FullName`; roles seeded at startup are **`User`** and **`Admin`**.
- JWT bearer authentication uses HMAC-SHA256 with `Jwt:SecretKey` (minimum 32 UTF-8 bytes —
  startup throws if missing or too short), validates issuer, audience, lifetime and signing key,
  with a one-minute clock skew. Claims include `sub`, `NameIdentifier`, `Name`, `Email`,
  `fullName` and one `Role` claim per role. Default expiry is 60 minutes
  (`Jwt:ExpiryMinutes`).
- Public registration always creates a `User`. Login is case-insensitive for usernames and
  returns the JWT session used by the frontend.
- The frontend stores the session in `ProtectedSessionStorage`, restores it once per Blazor
  circuit, derives roles from the JWT claims, and sends `Authorization: Bearer <token>` on every
  API call via `ApiClient`. `JwtAuthenticationStateProvider` exposes the session to Blazor's
  `[Authorize]`/cascading authentication state, and `AuthStateChanged` notifies pages after
  login, register or logout.

---

## Frontend

### Service layer (`EsportPlatform.WEB/Services`)

| Service | Responsibility |
| --- | --- |
| `ApiHttpClient` | Named client: base URL from `Api:BaseUrl`, 10 s timeout, 8 MB response buffer |
| `ApiClient` | Generic GET, `GetAllAsync` pagination walker, and send (POST/PUT/PATCH/DELETE) with bearer token |
| `ApiDataService` | Scoped, shared cache of games/teams/players/tournaments/matches/highlights for pages; exposes counts and API-availability state |
| `GameApiService`, `TeamApiService`, `PlayerApiService`, `TournamentApiService`, `MatchApiService`, `HighlightApiService` | Resource-specific API calls and API-DTO ↔ view-model mapping |
| `AuthService` | Login/register/logout, session restore from protected storage, JWT role parsing, readable API errors |
| `JwtAuthenticationStateProvider` | Blazor authentication state from the JWT session |
| `BaseApiService`, `GameArt`, `Models/*` | Shared helpers and client-side view models |

### UI structure

```
Components/
  App.razor, Routes.razor          # Interactive Server host, router, NotFound page
  Layout/
    PublicLayout.razor             # public top navigation
    AdminLayout.razor, NavMenu.razor  # admin sidebar shell and navigation
    MainLayout.razor               # base layout
    ReconnectModal.razor (+ .js)   # Blazor SignalR reconnect UX
  Pages/
    Home, Games, Tournaments, Teams, Players, Scores, Highlights,
    Register, Login, SignUp, Account, NotFound, Error, ServerError
    Admin/
      Dashboard, Games, Teams, Players, Tournaments, Matches, Highlights
  Shared/
    GameBanner.razor, StatusChip.razor, ApiDataPage.cs
wwwroot/app.css                    # theme tokens and component styles
```

Design system: near-black surfaces (`#08080a`/`#111114`), indigo accent (`#6d5ef8`) with cyan,
green, amber and rose for status, soft borders and layered shadows, Sora headings / Inter body /
JetBrains Mono scores and data labels, responsive public nav and collapsing admin sidebar. All
theme tokens are under `:root` in `wwwroot/app.css`.

More frontend-only detail is in `EsportPlatform.WEB/README-FRONTEND.md`.

---

## Getting started

### Prerequisites

- .NET 10 SDK (verified with 10.0.403)
- No external database server needed (SQLite file is created automatically)
- Optional: Playwright browsers for the E2E tests

### Run the application

Start the API first, then the web app, in two terminals:

```bash
dotnet run --project EsportPlatform.API
dotnet run --project EsportPlatform.WEB
```

Then open **http://localhost:5146**. The API root at **http://localhost:5040/** should return
`{"name":"EGame Jams","status":"ok",...}`.

On startup the API applies pending EF Core migrations, seeds the demo dataset if the database is
empty, creates the `User` and `Admin` roles, and optionally creates an admin account from the
`Admin` configuration section (in Development this is set in
`EsportPlatform.API/appsettings.Development.json`). Sign in on `/login` to use the admin area.

Useful commands:

```bash
# Build individual projects
dotnet build EsportPlatform.API/EsportPlatform.API.csproj
dotnet build EsportPlatform.WEB/EsportPlatform.WEB.csproj

# Add a migration / update the database manually (from the API folder)
dotnet ef migrations add <Name>
dotnet ef database update

# Run tests
dotnet test EsportPlatform.Tests/EsportPlatform.Tests.csproj
```

---

## Configuration

| Setting | File | Purpose |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | `EsportPlatform.API/appsettings.json` | SQLite connection (`Data Source=esports.db`) |
| `Jwt:Issuer`, `Jwt:Audience` | `EsportPlatform.API/appsettings.json` | Token issuer/audience validation |
| `Jwt:SecretKey` | `EsportPlatform.API/appsettings.json` | HMAC signing key, ≥32 bytes |
| `Jwt:ExpiryMinutes` | `EsportPlatform.API/appsettings.json` | Token lifetime (default 60) |
| `Admin:UserName`, `Admin:Password`, `Admin:FullName` | `EsportPlatform.API/appsettings.Development.json` | Optional seeded admin account; seeding is skipped when user/password are empty |
| `Api:BaseUrl` | `EsportPlatform.WEB/appsettings.json` / `.Development.json` | API base address used by the named HttpClient |
| CORS `AllowBlazorClient` | `EsportPlatform.API/Program.cs` | Allows `http://localhost:5146`, any header/method |
| Launch profiles | `*/Properties/launchSettings.json` | API `5040`/`7254`, WEB `5146`/`7026` |

The SQLite file (`EsportPlatform.API/esports.db`), data-protection key folders, build output and
local secrets are gitignored.

---

## Database, migrations and seed data

`AppDbContext` inherits `IdentityDbContext<ApplicationUser>` and exposes `Games`, `Tournaments`,
`Teams`, `Matches`, `Players`, `Highlights` and `TournamentTeams`. `Program.cs` calls
`Database.MigrateAsync()` at startup, so migrations in `EsportPlatform.API/Migrations/` are
applied automatically. They evolve the schema from the initial create through unique game names,
relationship fixes, the complete esports domain, broadcast/highlight changes, match scores,
Identity tables and tournament memberships.

`DbInitializer.SeedAsync` loads demo data once on a fresh database (8 games, 12 teams, 17
players, 6 tournaments, 7 matches, 6 highlights) and backfills `TournamentTeam` memberships from
scheduled matches. It deliberately preserves databases that already contain real user data, and
has helpers to add highlights/memberships to older seeded databases without overwriting other
records.

---

## Testing

The test project contains **103 test cases** across eight files:

| File | Coverage |
| --- | --- |
| `AuthControllerTest.cs` | Registration (role assignment, duplicate username, weak/mismatched passwords), login, JWT claims (8 tests) |
| `GameControllerTest.cs` | Game list/get/create/update/duplicate detection (9 tests) |
| `TeamControllerTest.cs` | CRUD, status, team registration with roster + tournament slot, roster capacity (21 tests) |
| `PlayerControllerTest.cs` | CRUD, gamertag uniqueness, roster count adjustments, statuses (17 tests) |
| `TournamentControllerTest.cs` | CRUD, duplicate names, invalid game (13 tests) |
| `MatchControllerTest.cs` | Scheduling validation, CRUD, statuses, scores (19 tests) |
| `HighlightControllerTest.cs` | CRUD, required fields, invalid game (12 tests) |
| `GeneralFunctionalityTests.cs` | Playwright/HTTP smoke tests: API root, public endpoints, home page and public routes (4 tests, `[Trait("Category","E2E")]`) |

Run the controller tests with `dotnet test EsportPlatform.Tests/EsportPlatform.Tests.csproj`.
The E2E tests require both apps to be running and Playwright browsers installed:

```bash
pwsh EsportPlatform.Tests/bin/Debug/net10.0/playwright.ps1 install   # or: playwright install chromium
export E2E_API_BASE_URL=http://localhost:5040
export E2E_FRONTEND_BASE_URL=http://localhost:5146
dotnet test EsportPlatform.Tests/EsportPlatform.Tests.csproj --filter Category=E2E
```

> **Current status:** `EsportPlatform.API` and `EsportPlatform.WEB` both build cleanly. The test
> project was written against an earlier version of the API (services constructed with
> `AppDbContext`, entity objects passed directly to controller methods) and, after the
> service/repository/DTO refactor, no longer compiles — `dotnet build` currently reports type
> errors in the controller test files. `AuthControllerTest` and the E2E `GeneralFunctionalityTests`
> are compatible with the current API. Updating the remaining tests to use `*Dto` request records
> and repository-backed service constructors is the active work item in `notes.md` (step 8).
>
> The project follows a red/green/refactor workflow: write a failing test for a missing case,
> change the code until it passes, then clean up.

---

## Project structure

```
esportplatform_with_tests/
├── EsportPlatform.API/                 REST backend
│   ├── Controllers/                    Game, Team, Tournament, Match, Player, Highlight, Auth/
│   ├── services/                       Business logic (one service per resource)
│   ├── Interfaces/
│   │   ├── services.Interfaces/        IGameService, ITeamService, ...
│   │   └── repositories.interfaces/    IGameRepository, ITeamRepository, ...
│   ├── Data/
│   │   ├── DTOs/                       Request/response DTOs per resource + auth
│   │   ├── Repositories/               EF Core queries, generic Repository<T>
│   │   ├── Seeders/AdminSeeder.cs      Optional admin account seeding
│   │   ├── AppDbContext.cs             EF Core model + relationships
│   │   └── DbInitializer.cs            Demo dataset
│   ├── Identity/                       ApplicationUser, JwtSettings
│   ├── Models/                         EF entities
│   ├── Contracts/                      PaginationQuery, PagedResult<T>, status/registration requests
│   ├── Migrations/                     EF Core migrations + snapshot
│   ├── Program.cs                      DI, JWT/Identity, CORS, seeding, middleware, endpoints
│   └── appsettings*.json               Connection string, JWT, dev admin account
├── EsportPlatform.WEB/                 Blazor front end
│   ├── Components/{Layout,Pages,Shared}/  UI, admin area, reusable components
│   ├── Services/                       API clients, auth, shared data
│   ├── Models/                         Client-side view models
│   ├── wwwroot/                        app.css, static assets
│   ├── Program.cs                      DI, auth, HttpClient, middleware
│   └── README-FRONTEND.md              Frontend-focused documentation
├── EsportPlatform.Tests/               xUnit + Playwright tests
├── notes.md                            Architecture roadmap and TDD notes
└── README.md                           This document
```

---

## Roadmap, known gaps and production notes

`notes.md` records the refactoring roadmap: model/controller API → services → DTOs → repositories
→ interfaces → error/validation pipeline → DI/IoC → unit & integration testing. The first seven
steps are reflected in the current source; step 8 (testing) is in progress. Concrete gaps to be
aware of:

- The controller tests still use pre-refactor signatures and need to be migrated to DTOs and
  repository-backed service constructors before the test project builds.
- Request validation is currently manual in controllers plus DataAnnotations on auth DTOs; there
  is no centralized exception-handling middleware registered in `Program.cs` yet.
- The checked-in JWT secret and Development admin credentials are for local development only.
  Move secrets to user-secrets/environment variables, set a strong issuer/audience/secret, and
  change or remove the seeded admin before deploying.
- The CORS origin and `Api:BaseUrl` are hardcoded to localhost; update both for a real deployment.
- Uploaded images are stored as byte arrays in SQLite; consider object storage for production.

---

*Documentation covers the repository state at the time of writing. For frontend-only details see
[`EsportPlatform.WEB/README-FRONTEND.md`](EsportPlatform.WEB/README-FRONTEND.md) and for the
refactoring history see [`notes.md`](notes.md).*
