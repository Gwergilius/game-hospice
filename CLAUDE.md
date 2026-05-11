# Game Hospice

A web-based framework for running local DOS games in the browser, powered by js-dos (WebAssembly DOSBox port) and served via Docker.

## Project Overview

Game Hospice scans a local directory of DOS game folders, presents them as a browsable library (with cover art and metadata), and lets the user launch any game directly in the browser — no installation required on the client side.

The application runs in Docker. The host machine's game directory is mounted as a read-only volume. The browser communicates with an ASP.NET Core Minimal API that serves game metadata and on-the-fly ZIP archives. The browser-side js-dos runtime loads each ZIP and runs the game via WebAssembly DOSBox.

## Language Policy

- **All code, comments, documentation, commit messages, and identifiers: English**
- Hungarian is used exclusively in chat/prompt communication

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Language | C# 14 |
| Frontend | Blazor (WebAssembly preferred) |
| DOS emulator | js-dos v8 (DOSBox-X compiled to WASM) |
| Containerization | Docker + Docker Compose |
| Target repository | GitHub (`game-hospice`) |

## Architecture

```
Browser (Blazor WASM)
  │
  ├─ GET /api/games          → game list (title, cover URL, metadata)
  ├─ GET /api/games/{id}/cover → cover image
  └─ GET /api/games/{id}/zip   → on-the-fly ZIP fed to js-dos
                                 │
                         ASP.NET Core Minimal API (Docker)
                                 │
                         /games volume (read-only)
                                 │
                         Host machine: local DOS game folders
```

## Game Directory Convention

Each game lives in its own subdirectory under the configured games root:

```
/games/
├── Commander Keen/
│   ├── KEEN1.EXE
│   ├── *.* (game files)
│   ├── dosbox.conf     ← optional; auto-generated if missing
│   ├── cover.png       ← optional cover image (any extension)
│   └── game.json       ← optional metadata
├── DOOM/
│   ├── DOOM.EXE
│   └── cover.jpg
└── Wolf3D/
    ├── WOLF3D.EXE
    ├── game.json
    └── cover.png
```

### game.json schema

```json
{
  "title": "Commander Keen: Marooned on Mars",
  "year": 1990,
  "genre": "Platformer",
  "exe": "KEEN1.EXE",
  "dosboxArgs": "-conf dosbox.conf"
}
```

All fields are optional. If `game.json` is absent, the folder name is used as the title and the main `.EXE` is auto-detected.

## Planned Project Structure

```
Game-hospice/
├── CLAUDE.md
├── GameHospice.sln
├── docker-compose.yml
├── Dockerfile
├── docs/
│   └── DOS játékok böngészőben.hu.md   ← original design conversation (Hungarian)
│
├── src/
│   ├── GameHospice.Api/                ← ASP.NET Core Minimal API
│   │   ├── Models/
│   │   ├── Services/
│   │   │   ├── GameScanner.cs
│   │   │   └── GameZipper.cs
│   │   ├── Endpoints/
│   │   └── Program.cs
│   │
│   └── GameHospice.Web/                ← Blazor WebAssembly frontend
│       ├── Pages/
│       │   ├── Library.razor           ← game grid / browser
│       │   └── Play.razor              ← js-dos player
│       ├── Services/
│       └── wwwroot/
│
└── tests/
    └── GameHospice.Api.Tests/
```

## Key Design Decisions

- **On-the-fly ZIP**: The API never stores temporary ZIPs on disk. Archives are created in memory per request and streamed directly to the browser.
- **Auto dosbox.conf generation**: If a game folder lacks `dosbox.conf`, the API heuristically finds the main `.EXE` and generates a minimal config.
- **Read-only volume**: The games directory is mounted `:ro` in Docker — the application never writes to it.
- **Blazor WASM preferred**: Running the frontend as WebAssembly avoids a persistent server connection and keeps the Docker footprint to the API only. Revisit if the game catalog scanning or SignalR-based features require server-side Blazor.

## Development

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- A local folder of DOSBox-compatible game directories

### Running locally with Docker

```bash
docker compose up --build
```

Map your games folder in `docker-compose.yml`:

```yaml
volumes:
  - C:/DOS/Games:/games:ro
```

Then open `http://localhost:8080`.

### Running without Docker (development)

```bash
dotnet run --project src/GameHospice.Api
# set GamesPath env var or appsettings.Development.json
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `GamesPath` | `/games` | Path to the root games directory (inside the container) |
| `ASPNETCORE_URLS` | `http://+:8080` | API listen address |
