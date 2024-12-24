using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Homepage.Models;

namespace DashiToon.Api.Application.Homepage.Queries.GetRecommendSeries;

[Authorize]
public sealed record GetRecommendedSeriesQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PaginatedList<SeriesInfoVm>>;

public sealed class GetRecommendSeriesQueryHandler
    : IRequestHandler<GetRecommendedSeriesQuery, PaginatedList<SeriesInfoVm>>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly IHomepageRepository _homepageRepository;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;

    public GetRecommendSeriesQueryHandler(
        IAnalyticRepository analyticRepository,
        IHomepageRepository homepageRepository,
        IImageStore imageStore,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _homepageRepository = homepageRepository;
        _imageStore = imageStore;
        _user = user;
    }

    public async Task<PaginatedList<SeriesInfoVm>> Handle(GetRecommendedSeriesQuery request,
        CancellationToken cancellationToken)
    {
        IEnumerable<UserLikedSeriesDto> userLikedSeries = await _homepageRepository.GetUserLikedSeries();

        int[] recommendSeriesIds = RecommendationService.GetRecommendations(
            _user.Id!,
            userLikedSeries,
            request.PageNumber * request.PageSize);


        if (recommendSeriesIds.Length == 0)
        {
            recommendSeriesIds = (await _analyticRepository.GetTopSeries("year")).ToArray();
        }

        IEnumerable<SeriesInfoDto>? recommendedSeries = await _homepageRepository.GetSeries(
            recommendSeriesIds,
            request.PageNumber,
            request.PageSize);

        List<SeriesInfoVm>? result = new();

        foreach (SeriesInfoDto? series in recommendedSeries)
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

        return new PaginatedList<SeriesInfoVm>(
            result.ToList(),
            recommendSeriesIds.Length,
            request.PageNumber,
            request.PageSize);
    }
}
