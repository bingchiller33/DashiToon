using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Genres.Queries.GetGenres;
using DashiToon.Api.Application.Homepage.Models;
using DashiToon.Api.Application.Homepage.Queries.GetRecentlyReleasedSeries;
using DashiToon.Api.Application.Homepage.Queries.GetRecentlyUpdatedSeries;
using DashiToon.Api.Application.Homepage.Queries.GetTrendingSeries;
using DashiToon.Api.Application.Homepage.Queries.GetRecommendSeries;
using DashiToon.Api.Application.Homepage.Queries.GetTopGenres;
using DashiToon.Api.Application.Homepage.Queries.GetTrendingGenreSeries;

namespace DashiToon.Api.Web.Endpoints;

public class Homepage : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .MapGet(GetTrendingSeries, "trending-series")
            .MapGet(GetTrendingGenres, "trending-genres")
            .MapGet(GetTopGenresSeries, "trending-genres-series")
            .MapGet(GetRecentlyReleasedSeries, "recently-released-series")
            .MapGet(GetRecentlyUpdatedSeries, "recently-updated-series")
            .MapGet(GetRecommendedSeries, "recommended-series");
    }

    public async Task<List<SeriesInfoVm>> GetTrendingSeries(ISender sender, string interval = "week")
    {
        return await sender.Send(new GetTrendingSeriesQuery(interval));
    }

    public async Task<List<GenreVm>> GetTrendingGenres(ISender sender)
    {
        return await sender.Send(new GetTopGenresQuery());
    }

    public async Task<List<SeriesInfoVm>> GetTopGenresSeries(ISender sender, int genreId)
    {
        return await sender.Send(new GetTrendingGenreSeriesQuery(genreId));
    }

    public async Task<List<SeriesInfoVm>> GetRecentlyReleasedSeries(ISender sender)
    {
        return await sender.Send(new GetRecentlyReleasedSeriesQuery());
    }

    public async Task<List<RecentlyUpdatedSeriesVm>> GetRecentlyUpdatedSeries(ISender sender)
    {
        return await sender.Send(new GetRecentlyUpdatedSeriesQuery());
    }

    public async Task<PaginatedList<SeriesInfoVm>> GetRecommendedSeries(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetRecommendedSeriesQuery(pageNumber, pageSize));
    }
}
