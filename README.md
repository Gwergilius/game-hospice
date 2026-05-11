[js-dos]: https://js-dos.com "js-dos — DOSBox-X compiled to WebAssembly"
[js-dos-npm]: https://www.npmjs.com/package/js-dos "js-dos on npm"
[dosbox-x]: https://dosbox-x.matter.website "DOSBox-X — enhanced DOSBox fork"
[dotnet-10]: https://dotnet.microsoft.com/en-us/download/dotnet/10.0 ".NET 10 SDK download"
[blazor-wasm]: https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-10.0 "ASP.NET Core Blazor documentation"
[docker-desktop]: https://www.docker.com/products/docker-desktop "Docker Desktop"
[docker-compose-docs]: https://docs.docker.com/compose "Docker Compose documentation"
[github-repo]: https://github.com/Gwergilius/game-hospice "Game Hospice on GitHub"

# Game Hospice

A web-based framework for running local DOS games in the browser, powered by [js-dos] (WebAssembly [DOSBox-X][dosbox-x] port) and served via Docker.

## Project Overview

Game Hospice scans a local directory of DOS game folders, presents them as a browsable library (with cover art and metadata), and lets the user launch any game directly in the browser — no installation required on the client side.

The application runs in [Docker][docker-desktop]. The host machine's game directory is mounted as a read-only volume. The browser communicates with an ASP.NET Core Minimal API that serves game metadata and on-the-fly ZIP archives. The browser-side [js-dos] runtime loads each ZIP and runs the game via WebAssembly DOSBox.

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | [.NET 10][dotnet-10] |
| Language | C# 14 |
| Frontend | [Blazor WebAssembly][blazor-wasm] |
| DOS emulator | [js-dos] v8 ([DOSBox-X][dosbox-x] compiled to WASM) |
| Containerization | [Docker Desktop][docker-desktop] + [Docker Compose][docker-compose-docs] |

## Architecture

```
Browser (Blazor WASM)
  │
  ├─ GET /api/games            → game list (title, cover URL, metadata)
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

### game.json

All fields are optional. If `game.json` is absent, the folder name is used as the title and the main `.EXE` is auto-detected.

```json
{
  "title": "Commander Keen: Marooned on Mars",
  "year": 1990,
  "genre": "Platformer",
  "exe": "KEEN1.EXE",
  "dosboxArgs": "-conf dosbox.conf"
}
```

## Development

### Prerequisites

- [.NET 10 SDK][dotnet-10]
- [Docker Desktop][docker-desktop]
- A local folder of DOSBox-compatible game directories

### Running with Docker

Edit `docker-compose.yml` to point to your games folder:

```yaml
volumes:
  - C:/DOS/Games:/games:ro
```

Then start the stack:

```bash
docker compose up --build
```

Open `http://localhost:8080`.

### Running without Docker

```bash
dotnet run --project src/GameHospice.Api
```

Set `GamesPath` via environment variable or `appsettings.Development.json`:

```json
{
  "GamesPath": "C:/DOS/Games"
}
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `GamesPath` | `/games` | Path to the games root directory (inside the container) |
| `ASPNETCORE_URLS` | `http://+:8080` | API listen address |
