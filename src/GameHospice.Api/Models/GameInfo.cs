namespace GameHospice.Api.Models;

public sealed record GameInfo(
    string Id,
    string Title,
    string? Year,
    string? Company,
    string? Genre,
    string? Cover);
