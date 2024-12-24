using DashiToon.Api.Application.Search.Models;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface ISearchService
{
    Task<bool> IndexSeriesAsync(Series series);
    Task<bool> UpdateSeriesAsync(Series series);
    Task<bool> DeleteSeriesAsync(int id);
    Task<bool> BulkUpdateViewCountAsync(Dictionary<int, int> seriesViewCount);
    Task<bool> UpdateSeriesRatingAsync(int seriesId, double rating);
    Task<SeriesSearchResult> SearchSeriesAsync(SeriesSearchOptions searchOptions);
}
