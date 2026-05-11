using System.IO.Compression;
using GameHospice.Api.Models;

namespace GameHospice.Api.Services;

public sealed partial class GameZipper(IConfiguration configuration, ILogger<GameZipper> logger)
{
    private static readonly string[] ExeExclusions = ["setup", "install", "uninst", "uninstall", "deinstal"];

    private readonly string _gamesPath = configuration["GamesPath"]
        ?? throw new InvalidOperationException("GamesPath configuration is required.");

    public (Stream ZipStream, string FileName) BuildZip(string gameId)
    {
        var gamesRoot = Path.GetFullPath(_gamesPath);
        var gameDir = Path.GetFullPath(Path.Combine(gamesRoot, gameId));

        if (!gameDir.StartsWith(gamesRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Invalid game ID.", nameof(gameId));

        if (!Directory.Exists(gameDir))
            throw new DirectoryNotFoundException($"Game not found: {gameId}");

        var meta = GameMetadata.Load(gameDir);
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "game.json",
            meta?.Cover ?? "cover.jpg"
        };

        var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var hasDosboxConf = false;

            foreach (var file in Directory.EnumerateFiles(gameDir, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(gameDir, file).Replace('\\', '/');

                if (excluded.Contains(relative))
                    continue;

                if (relative.Equals("dosbox.conf", StringComparison.OrdinalIgnoreCase))
                    hasDosboxConf = true;

                archive.CreateEntryFromFile(file, relative);
            }

            if (!hasDosboxConf)
            {
                var conf = GenerateDosboxConf(gameDir, gameId, meta);
                var entry = archive.CreateEntry("dosbox.conf");
                using var writer = new StreamWriter(entry.Open());
                writer.Write(conf);
            }
        }

        ms.Position = 0;
        return (ms, $"{gameId}.zip");
    }

    private string GenerateDosboxConf(string gameDir, string gameId, GameMetadata? meta)
    {
        var exe = meta?.Command ?? FindMainExe(gameDir, gameId);
        LogAutoGeneratingDosboxConf(gameId, exe ?? "none");

        var lines = new List<string> { "[autoexec]", "@echo off", "mount c .", "c:" };
        if (exe is not null)
            lines.Add(exe);

        return string.Join('\n', lines) + '\n';
    }

    private static string? FindMainExe(string gameDir, string gameId)
    {
        var exes = Directory.EnumerateFiles(gameDir, "*.exe", SearchOption.TopDirectoryOnly)
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(n => !ExeExclusions.Any(ex => n.Contains(ex, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (exes.Count == 0) return null;

        var nameMatch = exes.FirstOrDefault(e =>
            Path.GetFileNameWithoutExtension(e).Equals(gameId, StringComparison.OrdinalIgnoreCase));
        if (nameMatch is not null) return nameMatch;

        return exes
            .Select(e => new FileInfo(Path.Combine(gameDir, e)))
            .MaxBy(f => f.Length)
            ?.Name;
    }

    [LoggerMessage(Level = LogLevel.Information,
        Message = "Auto-generating dosbox.conf for {GameId}, main exe: {Exe}")]
    private partial void LogAutoGeneratingDosboxConf(string gameId, string exe);
}
