using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Search.Models;

public class SearchModel
{
    public int Id { get; init; }
    public string? Thumbnail { get; set; }
    public required string Title { get; init; }
    public SeriesType Type { get; init; }
    public SeriesStatus Status { get; init; }
    public ContentRating ContentRating { get; init; }
    public string[] AlternativeTitles { get; init; } = [];
    public string[] Authors { get; init; } = [];
    public string[] Genres { get; init; } = [];
    public DateTimeOffset? StartTime { get; init; }
    public int ViewCount { get; init; }
    public double Rating { get; init; }

    public static SearchModel FromEntity(Series series, int? viewCount = null, double? rating = null)
    {
        return new SearchModel
        {
            Id = series.Id,
            Thumbnail = series.Thumbnail,
            Title = series.Title,
            Type = series.Type,
            Status = series.Status,
            ContentRating = series.ContentRating,
            AlternativeTitles = series.AlternativeTitles,
            Authors = series.Authors,
            Genres = series.Genres.Select(g => g.Name).ToArray(),
            StartTime = series.StartTime,
            ViewCount = viewCount ?? 0,
            Rating = rating ?? 0
        };
    }
}
