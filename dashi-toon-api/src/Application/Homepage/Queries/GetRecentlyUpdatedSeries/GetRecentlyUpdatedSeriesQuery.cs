using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Homepage.Models;

namespace DashiToon.Api.Application.Homepage.Queries.GetRecentlyUpdatedSeries;

public sealed record GetRecentlyUpdatedSeriesQuery : IRequest<List<RecentlyUpdatedSeriesVm>>; // Request

public sealed class GetRecentlyUpdatedSeriesQueryHandler
    : IRequestHandler<GetRecentlyUpdatedSeriesQuery, List<RecentlyUpdatedSeriesVm>>
{
    private readonly IHomepageRepository _homepageRepository;
    private readonly IImageStore _imageStore;

    public GetRecentlyUpdatedSeriesQueryHandler(IHomepageRepository homepageRepository, IImageStore imageStore)
    {
        _homepageRepository = homepageRepository;
        _imageStore = imageStore;
    }

    public async Task<List<RecentlyUpdatedSeriesVm>> Handle(GetRecentlyUpdatedSeriesQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<RecentlyUpdatedSeriesDto>? recentlyUpdatedSeries =
            await _homepageRepository.GetRecentlyUpdatedSeries();

        List<RecentlyUpdatedSeriesVm>? result = new();

        foreach (RecentlyUpdatedSeriesDto? series in recentlyUpdatedSeries)
        {
            result.Add(new RecentlyUpdatedSeriesVm(
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
                series.ContentRating,
                series.ChapterId,
                series.ChapterNumber,
                series.ChapterName,
                series.PublishedDate.ToString("O")
            ));
        }

        return result;
    }
}
