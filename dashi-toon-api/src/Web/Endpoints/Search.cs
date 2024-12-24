using DashiToon.Api.Application.Search.Models;
using DashiToon.Api.Application.Search.Queries.SearchSeries;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Web.Endpoints;

public class Search : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app
            .MapGroup(this)
            .MapGet(SearchSeries);
    }

    public async Task<SeriesSearchResult> SearchSeries(
        ISender sender,
        string? term,
        SeriesType[]? type,
        SeriesStatus[]? status,
        ContentRating[]? contentRating,
        string[]? genres,
        string sortBy = "trending",
        string sortOrder = "asc",
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new SearchSeriesQuery(
            term,
            type ?? [],
            status ?? [],
            contentRating ?? [],
            genres ?? [],
            sortBy,
            sortOrder,
            pageNumber,
            pageSize));
    }
}
