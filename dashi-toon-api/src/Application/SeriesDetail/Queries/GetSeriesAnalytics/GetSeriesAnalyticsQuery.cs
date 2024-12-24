using DashiToon.Api.Application.Common.Interfaces;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesAnalytics;

public sealed record GetSeriesAnalyticsQuery(int Id) : IRequest<SeriesAnalyticsVm>;

public sealed class GetSeriesAnalyticsQueryHandler : IRequestHandler<GetSeriesAnalyticsQuery, SeriesAnalyticsVm>
{
    private readonly ISeriesRepository _seriesRepository;

    public GetSeriesAnalyticsQueryHandler(ISeriesRepository seriesRepository)
    {
        _seriesRepository = seriesRepository;
    }

    public async Task<SeriesAnalyticsVm> Handle(GetSeriesAnalyticsQuery request, CancellationToken cancellationToken)
    {
        SeriesAnalyticsDto analytics = await _seriesRepository.GetSeriesAnalytics(request.Id);

        return new SeriesAnalyticsVm(
            analytics.ReviewCount == 0
                ? 0
                : Math.Round(
                    (double)analytics.RecommendedCount * 100 / analytics.ReviewCount,
                    2,
                    MidpointRounding.ToPositiveInfinity),
            analytics.ReviewCount,
            analytics.FollowCount,
            analytics.ViewCount,
            analytics.LastModified.ToString("O")
        );
    }
}
