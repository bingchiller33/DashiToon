using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetRelatedSeries;

public sealed record RelatedSeriesVm(
    int Id,
    string Title,
    string[] AlternativeTitles,
    string[] Authors,
    float Rating,
    SeriesStatus Status,
    string Thumbnail,
    SeriesType Type,
    string[] Genres,
    ContentRating ContentRating
);

public sealed class RelatedSeriesDto
{
    public int Id { get; init; }
    public string? Thumbnail { get; init; }
    public string[] AlternativeTitles { get; init; } = [];
    public string[] Authors { get; init; } = [];
    public float Rating { get; set; }
    public required string Title { get; init; }
    public SeriesType Type { get; init; }
    public SeriesStatus Status { get; init; }
    public ContentRating ContentRating { get; init; }
    public int[] GenresMap { get; init; } = [];
    public string[] Genres { get; init; } = [];
    public DateTimeOffset? StartTime { get; init; }
}
