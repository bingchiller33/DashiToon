using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Series.Queries.GetSeries;

public record SeriesDetailVm(
    int Id,
    string Title,
    string[] AlternativeTitles,
    string[] Authors,
    string StartTime,
    SeriesStatus Status,
    string Synopsis,
    string Thumbnail,
    SeriesType Type,
    List<string> Genres,
    ContentRating ContentRating,
    List<CategoryRatingVm> CategoryRatings);

public record CategoryRatingVm(string Category, int Option);
