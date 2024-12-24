using DashiToon.Api.Application.SeriesDetail.Queries.GetRelatedSeries;
using DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesAnalytics;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface ISeriesRepository
{
    Task<Series?> FindSeriesById(int seriesId);
    Task<Series?> FindSeriesWithVolumesAndChaptersById(int seriesId);
    Task<IEnumerable<RelatedSeriesDto>> GetRelatedSeries(int seriesId);
    Task<SeriesAnalyticsDto> GetSeriesAnalytics(int seriesId);
    Task<double> GetSeriesRating(int seriesId);
}
