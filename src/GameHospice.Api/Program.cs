using GameHospice.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<GameScanner>();
builder.Services.AddSingleton<GameZipper>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.MapGet("/api/games", (GameScanner scanner) => scanner.List())
    .WithName("GetGames");

app.MapGet("/api/games/{id}/cover", IResult (string id, GameScanner scanner) =>
{
    var path = scanner.GetCoverPath(id);
    if (path is null) return Results.NotFound();

    var mime = Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png"            => "image/png",
        ".gif"            => "image/gif",
        ".webp"           => "image/webp",
        _                 => "application/octet-stream"
    };
    return Results.File(path, mime);
})
.WithName("GetGameCover");

app.MapGet("/api/games/{id}/download", IResult (string id, GameZipper zipper) =>
{
    try
    {
        var (stream, fileName) = zipper.BuildZip(id);
        return Results.File(stream, "application/zip", fileName);
    }
    catch (DirectoryNotFoundException)
    {
        return Results.NotFound();
    }
    catch (ArgumentException)
    {
        return Results.BadRequest();
    }
})
.WithName("DownloadGame");

app.Run();
