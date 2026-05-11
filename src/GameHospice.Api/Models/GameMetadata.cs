using System.Text.Json;

namespace GameHospice.Api.Models;

public sealed class GameMetadata
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public string? Title { get; init; }
    public string? Year { get; init; }
    public string? Company { get; init; }
    public string? Genre { get; init; }
    public string? Cover { get; init; }
    public string? Command { get; init; }

    public static GameMetadata? Load(string gameDir)
    {
        var path = Path.Combine(gameDir, "game.json");
        if (!File.Exists(path)) return null;
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<GameMetadata>(stream, JsonOptions);
    }
}
