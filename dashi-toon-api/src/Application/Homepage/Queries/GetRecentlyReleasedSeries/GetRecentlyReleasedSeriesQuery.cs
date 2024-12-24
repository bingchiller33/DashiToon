using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Homepage.Models;

namespace DashiToon.Api.Application.Homepage.Queries.GetRecentlyReleasedSeries;

public sealed record GetRecentlyReleasedSeriesQuery : IRequest<List<SeriesInfoVm>>; // Request

public sealed class GetRecentlyReleasedSeriesQueryHandler
    : IRequestHandler<GetRecentlyReleasedSeriesQuery, List<SeriesInfoVm>>
{
    private readonly IHomepageRepository _homepageRepository;
    private readonly IImageStore _imageStore;

    public GetRecentlyReleasedSeriesQueryHandler(IHomepageRepository homepageRepository, IImageStore imageStore)
    {
        _homepageRepository = homepageRepository;
        _imageStore = imageStore;
    }

    public async Task<List<SeriesInfoVm>> Handle(GetRecentlyReleasedSeriesQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<SeriesInfoDto>? recentlyReleasedSeries = await _homepageRepository.GetRecentlyReleasedSeries();

        List<SeriesInfoVm>? result = new();

        foreach (SeriesInfoDto? series in recentlyReleasedSeries)
        {
            result.Add(new SeriesInfoVm(
                series.Id,
                series.Title,
                series.AlternativeTitles,
                series.Authors,
                series.Rating,
                series.Status,
                string.IsNullOrEmpty(series.Thumbnail)
                    ? await _imageStore.GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                    : await _imageStore.GetUrl($"thumbnails/{series.Thumbnail}", DateTime.UtcNow.AddMinutes(2)),
                series.Type,
                series.Genres,
                series.ContentRating
            ));
        }

        return result;
    }
}
