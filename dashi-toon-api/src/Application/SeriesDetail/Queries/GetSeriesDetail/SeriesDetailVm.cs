using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDetail;

public sealed record SeriesDetailVm(
    int Id,
    string Title,
    IEnumerable<string> AlternativeTitles,
    string Author,
    SeriesStatus Status,
    string Synopsis,
    string Thumbnail,
    SeriesType Type,
    List<LookupDto> Genres,
    ContentRating ContentRating);
