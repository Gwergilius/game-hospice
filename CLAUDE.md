[readme]: README.md "Game Hospice README"
[readme-overview]: README.md#project-overview "Project Overview"
[readme-tech]: README.md#tech-stack "Tech Stack"
[readme-arch]: README.md#architecture "Architecture"
[readme-convention]: README.md#game-directory-convention "Game Directory Convention"
[readme-dev]: README.md#development "Development"
[github-repo]: https://github.com/Gwergilius/game-hospice "Game Hospice on GitHub"

# Game Hospice — Claude Guide

For a full project description see [README.md][readme] ([GitHub][github-repo]).

Quick references: [Overview][readme-overview] · [Tech Stack][readme-tech] · [Architecture][readme-arch] · [Game Directory Convention][readme-convention] · [Development][readme-dev]

## Language Policy

- **All code, comments, documentation, commit messages, and identifiers: English**
- Hungarian is used exclusively in chat/prompt communication

## Project Structure

```
Game-hospice/
├── CLAUDE.md
├── README.md
├── Directory.Build.props           ← C# 14, TreatWarningsAsErrors (solution-wide)
├── GameHospice.slnx
├── Dockerfile
├── docker-compose.yml
├── docs/
│   └── DOS játékok böngészőben.hu.md   ← original design conversation (Hungarian)
│
├── src/
│   ├── GameHospice.Api/                ← ASP.NET Core Minimal API
│   │   ├── Models/
│   │   ├── Services/
│   │   │   ├── GameScanner.cs          ← scans /games directory
│   │   │   └── GameZipper.cs           ← builds in-memory ZIP per request
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

- **On-the-fly ZIP**: The API never stores temporary ZIPs on disk. Archives are created in memory per request and streamed directly to the browser. This keeps the container stateless and the volume strictly read-only.
- **Auto `dosbox.conf` generation**: If a game folder lacks `dosbox.conf`, the API heuristically finds the main `.EXE` and generates a minimal config. The [game directory convention][readme-convention] describes how to override this.
- **Read-only volume**: The games directory is mounted `:ro` in Docker — the application never writes to the host game library.
- **Blazor WASM preferred**: Running the frontend as WebAssembly keeps the Docker footprint to a single API container. Revisit if game catalog streaming or real-time features require server-side Blazor.
