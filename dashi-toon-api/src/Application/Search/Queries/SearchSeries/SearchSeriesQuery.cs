using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Search.Models;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Search.Queries.SearchSeries;

public sealed record SearchSeriesQuery(
    string? SearchTerm,
    SeriesType[] Types,
    SeriesStatus[] Statuses,
    ContentRating[] ContentRatings,
    string[] Genres,
    string SortBy = "trending",
    string SortOrder = "asc",
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<SeriesSearchResult>;

public sealed class SearchSeriesQueryHandler : IRequestHandler<SearchSeriesQuery, SeriesSearchResult>
{
    private readonly ISearchService _searchService;
    private readonly IImageStore _imageStore;

    public SearchSeriesQueryHandler(ISearchService searchService, IImageStore imageStore)
    {
        _searchService = searchService;
        _imageStore = imageStore;
    }

    public async Task<SeriesSearchResult> Handle(SearchSeriesQuery request, CancellationToken cancellationToken)
    {
        SeriesSearchResult? result = await _searchService.SearchSeriesAsync(new SeriesSearchOptions
        {
            SearchTerm = request.SearchTerm,
            Types = request.Types,
            Statuses = request.Statuses,
            ContentRatings = request.ContentRatings,
            Genres = request.Genres,
            Page = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortOrder = request.SortOrder
        });

        List<SearchModel>? items = new(result.Items.Count);

        foreach (SearchModel? item in result.Items)
        {
            item.Thumbnail = string.IsNullOrEmpty(item.Thumbnail)
                ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                : await _imageStore.GetUrl($"thumbnails/{item.Thumbnail}", DateTime.UtcNow.AddMinutes(2));

            items.Add(item);
        }

        result.Items = items.AsReadOnly();

        return result;
    }
}
