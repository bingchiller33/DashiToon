using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Homepage.Models;

namespace DashiToon.Api.Application.Homepage.Queries.GetTrendingGenreSeries;

public sealed record GetTrendingGenreSeriesQuery(int GenreId) : IRequest<List<SeriesInfoVm>>;

public sealed class GetTrendingGenreSeriesQueryHandler
    : IRequestHandler<GetTrendingGenreSeriesQuery, List<SeriesInfoVm>>
{
    private readonly IHomepageRepository _homepageRepository;
    private readonly IAnalyticRepository _analyticRepository;
    private readonly IImageStore _imageStore;

    public GetTrendingGenreSeriesQueryHandler(
        IHomepageRepository homepageRepository,
        IAnalyticRepository analyticRepository,
        IImageStore imageStore)
    {
        _homepageRepository = homepageRepository;
        _analyticRepository = analyticRepository;
        _imageStore = imageStore;
    }

    public async Task<List<SeriesInfoVm>> Handle(GetTrendingGenreSeriesQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<int>? trendingSeriesId = await _analyticRepository.GetTopGenresSeries(request.GenreId);

        List<SeriesInfoDto>? series = (await _homepageRepository.GetSeries(trendingSeriesId.ToArray())).ToList();

        List<SeriesInfoVm>? result = new(series.Count);

        foreach (SeriesInfoDto? item in series)
        {
            result.Add(new SeriesInfoVm
            (
                item.Id,
                item.Title,
                item.AlternativeTitles,
                item.Authors,
                item.Rating,
                item.Status,
                string.IsNullOrEmpty(item.Thumbnail)
                    ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"thumbnails/{item.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
                item.Type,
                item.Genres,
                item.ContentRating
            ));
        }

        return result;
    }
}
