# DOS játékok böngészőben

**You said**:
Lehetséges-e valahogy régi DOS-os játékokat böngészőben játszani?
Valami olyasmi emulátorra gondolok, mint a DOSBOX, de ez böngészőben futna

---

**Claude said**:
DOS játékok böngészőben – js-dos / DOSBox-X (WebAssembly)
Igen, teljesen lehetséges! A kulcs a WebAssembly – a DOSBox portolva lett böngészőbe.
A főbb megoldások
1. js-dos v8 (legjobb minőségű, aktívan fejlesztett)

https://js-dos.com
A DOSBox-X WebAssembly-re fordítva
Saját játékot is feltölthetsz .zip-ben
Van React/Vue komponens, beágyazható saját weboldalba
Multiplayer támogatás (kísérleti)

2. Internet Archive – DOS Software Library

https://archive.org/details/softwarelibrary_msdos_games
Több ezer játék azonnal játszható, regisztráció nélkül
Ugyancsak js-dos a motor alatt
Klasszikusok: Doom, Commander Keen, Wolfenstein 3D, stb.

3. DOSBox Online (egyszerűbb alternatíva)

https://dosbox-online.com
Alapvetőbb, de könnyen kezelhető


Ha saját játékot akarsz futtatni (js-dos példa)
A js-dos NPM csomagként is elérhető, beágyazható Blazor/React projektbe:
html<!-- Legegyszerűbb embed -->
<script src="https://v8.js-dos.com/latest/js-dos.js"></script>
<div id="dos" style="width:640px; height:400px;"></div>

<script>
  Dos(document.getElementById("dos"), {
    url: "/games/keen1.zip",  // ZIP containing the game + dosbox.conf
  });
</script>
A ZIP struktúra:
keen1.zip
├── KEEN1/
│   ├── KEEN1.EXE
│   └── ...
└── dosbox.conf       ← autoexec szekció indítja a játékot
ini[autoexec]
mount c /
c:
cd KEEN1
KEEN1.EXE

Összefoglalva
MegoldásElőnyHátrányInternet ArchiveAzonnal, semmi telepítésCsak a katalógusukban lévő játékokjs-dos.comSaját ROM/game feltölthetőLassabb indulásjs-dos NPMBeágyazható saját appbaFejlesztői munka kell
Az Internet Archive a leggyorsabb út ha csak játszani szeretnél – az js-dos NPM csomag pedig ha mondjuk az ECHELON / SPARROW projekthez valami retró UI inspirációt keresel, vagy egy Blazor-ba ágyazott DOS emulátort álmodol meg. 😄

---

**You said**:
Amit szeretnék: Le vannak töltve a gépemre DOSBOX-os játékok. Ezeket szeretném futtatni böngészőből, pl. DOCKER-en keresztül. Írnék egy web alkalmazást, ami felajánlja a megtalált játékokat (cím, title-image), és rájuk kattintva a játék elindul a böngészőben.

---

**Claude said**:
DOS Játék Launcher – Architektúra terv
Ez egy klassz projekt! Gondoljuk végig az architektúrát mielőtt kódolunk.
A nagy kép
┌─────────────────────────────────────────────────────┐
│  Docker Container                                    │
│                                                      │
│  ┌──────────────────┐    ┌────────────────────────┐ │
│  │  ASP.NET Core    │    │  Static Frontend       │ │
│  │  Minimal API     │    │  HTML + js-dos (WASM)  │ │
│  │                  │    │                        │ │
│  │  /api/games      │    │  Játéklista grid       │ │
│  │  /api/games/{id}/│    │  → kattintás           │ │
│  │    zip           │    │  → js-dos betölti      │ │
│  │    cover         │    │    a ZIP-et az API-ból │ │
│  └────────┬─────────┘    └────────────────────────┘ │
│           │ volume mount                             │
└───────────┼─────────────────────────────────────────┘
            │
     /games (read-only)
            │
   ┌────────▼────────┐
   │  Helyi gép      │
   │  C:\DOS\Games\  │
   │  ├── Keen1\     │
   │  ├── Doom\      │
   │  └── Wolf3D\    │
   └─────────────────┘
Játék könyvtárstruktúra
A backend ezt a konvenciót követi:
/games/
├── Commander Keen/
│   ├── KEEN1.EXE
│   ├── *.* (game files)
│   ├── dosbox.conf          ← opcionális, ha nincs: auto-generál
│   ├── cover.png            ← opcionális borítókép
│   └── game.json            ← opcionális metaadat
│
├── DOOM/
│   ├── DOOM.EXE
│   └── cover.jpg
│
└── Wolf3D/
    ├── WOLF3D.EXE
    ├── game.json
    └── cover.png
game.json (opcionális, de hasznos):
json{
  "title": "Commander Keen: Marooned on Mars",
  "year": 1990,
  "genre": "Platformer",
  "exe": "KEEN1.EXE",
  "dosboxArgs": "-conf dosbox.conf"
}
Hogyan működik a js-dos integráció?
Browser                          API
  │                               │
  ├─ GET /api/games ─────────────►│ scan /games dirs
  │◄─ [{id, title, cover}] ───────┤
  │                               │
  │  [user clicks game]           │
  │                               │
  ├─ GET /api/games/Keen1/zip ───►│ ZIP-be csomagolja
  │◄─ keen1.zip ──────────────────┤ (memóriában, on-the-fly)
  │                               │
  │  js-dos betölti a ZIP-et      │
  │  WebAssembly DOSBox fut       │
  │  Játék indul!                 │
Projekt struktúra
DosLauncher/
├── DosLauncher.sln
├── docker-compose.yml
├── Dockerfile
│
├── DosLauncher.Api/            ← ASP.NET Core Minimal API
│   ├── Models/
│   │   └── GameInfo.cs
│   ├── Services/
│   │   ├── GameScanner.cs      ← /games könyvtár scan
│   │   └── GameZipper.cs       ← on-the-fly ZIP generálás
│   ├── Endpoints/
│   │   └── GameEndpoints.cs
│   └── Program.cs
│
└── DosLauncher.Web/            ← wwwroot static frontend
    ├── index.html              ← játéklista
    ├── play.html               ← js-dos player oldal
    ├── app.js
    └── style.css
Kulcs kódrészletek
GameInfo.cs
csharppublic record GameInfo(
    string Id,          // folder name, URL-safe
    string Title,       // display name
    string? CoverUrl,   // /api/games/{id}/cover vagy null
    int? Year,
    string? Genre
);
GameScanner.cs
csharppublic class GameScanner(IConfiguration config)
{
    private readonly string _gamesRoot = 
        config["GamesPath"] ?? "/games";

    public IEnumerable<GameInfo> ScanGames()
    {
        return Directory
            .EnumerateDirectories(_gamesRoot)
            .Select(dir => ParseGameDir(dir))
            .Where(g => g is not null)!;
    }

    private GameInfo? ParseGameDir(string dir)
    {
        var id = Path.GetFileName(dir);
        var metaFile = Path.Combine(dir, "game.json");
        
        // Try game.json first
        if (File.Exists(metaFile))
        {
            var meta = JsonSerializer.Deserialize<GameMeta>(
                File.ReadAllText(metaFile));
            return new GameInfo(
                Id: Uri.EscapeDataString(id),
                Title: meta?.Title ?? id,
                CoverUrl: HasCover(dir) ? $"/api/games/{Uri.EscapeDataString(id)}/cover" : null,
                Year: meta?.Year,
                Genre: meta?.Genre
            );
        }
        
        // Fallback: folder name as title
        return new GameInfo(
            Id: Uri.EscapeDataString(id),
            Title: id,
            CoverUrl: HasCover(dir) ? $"/api/games/{Uri.EscapeDataString(id)}/cover" : null,
            Year: null,
            Genre: null
        );
    }

    private static bool HasCover(string dir) =>
        Directory.EnumerateFiles(dir, "cover.*").Any();
}
GameZipper.cs – az on-the-fly ZIP trükk
csharppublic class GameZipper(IConfiguration config)
{
    private readonly string _gamesRoot = config["GamesPath"] ?? "/games";

    // js-dos needs a ZIP with dosbox.conf [autoexec] inside
    public async Task<byte[]> CreateGameZipAsync(string gameId)
    {
        var dir = Path.Combine(_gamesRoot, Uri.UnescapeDataString(gameId));
        
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            // Add all game files
            foreach (var file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
            {
                var entryName = Path.GetRelativePath(dir, file);
                archive.CreateEntryFromFile(file, entryName);
            }
            
            // Auto-generate dosbox.conf if missing
            if (!File.Exists(Path.Combine(dir, "dosbox.conf")))
            {
                var conf = GenerateDosboxConf(dir, gameId);
                var entry = archive.CreateEntry("dosbox.conf");
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(Encoding.UTF8.GetBytes(conf));
            }
        }
        
        return ms.ToArray();
    }

    private string GenerateDosboxConf(string dir, string gameId)
    {
        // Find the most likely .EXE to launch
        var exe = FindMainExe(dir);
        
        return $"""
            [dosbox]
            memsize=16
            
            [autoexec]
            mount c .
            c:
            {exe}
            """;
    }

    private static string FindMainExe(string dir)
    {
        var exeFiles = Directory.GetFiles(dir, "*.EXE", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .ToList();
        
        // Common patterns first
        string[] preferredNames = ["GAME.EXE", "PLAY.EXE", "START.EXE"];
        return exeFiles.FirstOrDefault(e => 
            preferredNames.Contains(e?.ToUpper())) 
            ?? exeFiles.First() 
            ?? "GAME.EXE";
    }
}
Program.cs – Minimal API endpoints
csharpvar builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<GameScanner>();
builder.Services.AddSingleton<GameZipper>();

var app = builder.Build();
app.UseStaticFiles(); // wwwroot

app.MapGet("/api/games", (GameScanner scanner) => 
    scanner.ScanGames());

app.MapGet("/api/games/{id}/zip", async (string id, GameZipper zipper) =>
{
    var zip = await zipper.CreateGameZipAsync(id);
    return Results.File(zip, "application/zip", $"{id}.zip");
});

app.MapGet("/api/games/{id}/cover", (string id, IConfiguration config) =>
{
    var dir = Path.Combine(config["GamesPath"] ?? "/games", 
        Uri.UnescapeDataString(id));
    var cover = Directory.EnumerateFiles(dir, "cover.*").FirstOrDefault();
    
    return cover is null 
        ? Results.NotFound() 
        : Results.File(cover, GetMimeType(cover));
});

app.Run();
docker-compose.yml
yamlservices:
  dos-launcher:
    build: .
    ports:
      - "8080:8080"
    volumes:
      - C:/DOS/Games:/games:ro    # ← ide jön a helyi games mappa
    environment:
      - GamesPath=/games
Dockerfile
dockerfileFROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DosLauncher.Api.dll"]
Frontend – play.html (js-dos integráció)
html<!-- A lényeg: js-dos betölti a ZIP-et az API-ból -->
<script src="https://v8.js-dos.com/latest/js-dos.js"></script>

<div id="dos-player"></div>

<script>
  const gameId = new URLSearchParams(location.search).get('id');
  
  Dos(document.getElementById('dos-player'), {
    url: `/api/games/${gameId}/zip`,
    // fullscreen gomb, billentyűzet layout, stb.
    onEvent: (event, ci) => {
      if (event === 'emu-ready') console.log('DOSBox ready!');
    }
  });
</script>

Következő lépések – érdemes ebben a sorrendben haladni

Projekt skeleton – solution, API projekt, docker-compose alap
GameScanner – könyvtár scan, tesztelés néhány játékkal
ZIP serving – GameZipper, tesztelés js-dos-szal manuálisan
Frontend – játéklista grid, kattintás, player oldal
Polishing – cover képek placeholder, dosbox.conf fine-tuning, hibakezelés

Elkezdjem valamelyik résszel a tényleges kódot kifejteni?

---

