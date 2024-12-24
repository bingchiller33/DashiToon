using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.AuthorStudio.Series.Queries.GetAllSeries;

public record SeriesVm(
    int Id,
    string Title,
    string[] AlternativeTitles,
    string[] Authors,
    string StartTime,
    SeriesStatus Status,
    string Thumbnail,
    SeriesType Type,
    List<string> Genres,
    ContentRating ContentRating,
    string UpdatedAt
);
