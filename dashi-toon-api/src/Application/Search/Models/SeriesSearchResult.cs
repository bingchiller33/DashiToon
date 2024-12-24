namespace DashiToon.Api.Application.Search.Models;

public class SeriesSearchResult
{
    public IReadOnlyCollection<SearchModel> Items { get; set; } = [];
    public long TotalCount { get; init; }
}
