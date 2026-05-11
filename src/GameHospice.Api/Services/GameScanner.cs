using GameHospice.Api.Models;

namespace GameHospice.Api.Services;

public sealed partial class GameScanner(IConfiguration configuration, ILogger<GameScanner> logger)
{
    private readonly string _gamesPath = configuration["GamesPath"]
        ?? throw new InvalidOperationException("GamesPath configuration is required.");

    public IReadOnlyList<GameInfo> List()
    {
        if (!Directory.Exists(_gamesPath))
        {
            LogGamesDirectoryNotFound(_gamesPath);
            return [];
        }

        return [.. Directory.EnumerateDirectories(_gamesPath)
            .Select(CreateGameInfo)
            .OrderBy(g => g.Title)];
    }

    public string? GetCoverPath(string gameId)
    {
        var gamesRoot = Path.GetFullPath(_gamesPath);
        var gameDir = Path.GetFullPath(Path.Combine(gamesRoot, gameId));

        if (!gameDir.StartsWith(gamesRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return null;
        if (!Directory.Exists(gameDir))
            return null;

        var meta = GameMetadata.Load(gameDir);
        var coverFile = meta?.Cover ?? "cover.jpg";
        var coverPath = Path.Combine(gameDir, coverFile);
        return File.Exists(coverPath) ? coverPath : null;
    }

    private static GameInfo CreateGameInfo(string directory)
    {
        var name = Path.GetFileName(directory)!;
        var meta = GameMetadata.Load(directory);

        var title = meta?.Title ?? name.Replace('_', ' ').Replace('-', ' ');
        var coverFile = meta?.Cover ?? "cover.jpg";
        var cover = File.Exists(Path.Combine(directory, coverFile)) ? coverFile : null;

        return new GameInfo(name, title, meta?.Year, meta?.Company, meta?.Genre, cover);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Games directory not found: {GamesPath}")]
    private partial void LogGamesDirectoryNotFound(string gamesPath);
}
