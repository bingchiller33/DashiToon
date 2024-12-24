using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Homepage.Models;

public sealed record SeriesInfoVm(
    int Id,
    string Title,
    List<string> AlternativeTitles,
    List<string> Authors,
    float Rating,
    SeriesStatus Status,
    string Thumbnail,
    SeriesType Type,
    List<string> Genres,
    ContentRating ContentRating
);

public sealed class SeriesInfoDto
{
    public int Id { get; init; }
    public required string Title { get; init; }
    public List<string> AlternativeTitles { get; init; } = [];
    public List<string> Authors { get; init; } = [];
    public float Rating { get; init; }
    public SeriesStatus Status { get; init; }
    public string? Thumbnail { get; init; }
    public SeriesType Type { get; init; }
    public List<string> Genres { get; init; } = [];
    public ContentRating ContentRating { get; init; }
}
