using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesReviewAnalytics;

[Authorize]
public sealed record GetSeriesReviewAnalyticsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<ReviewAnalyticsVm>;

public sealed class
    GetSeriesReviewAnalyticsQueryHandler : IRequestHandler<GetSeriesReviewAnalyticsQuery, ReviewAnalyticsVm>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUser _user;

    public GetSeriesReviewAnalyticsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<ReviewAnalyticsVm> Handle(
        GetSeriesReviewAnalyticsQuery request,
        CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _seriesRepository.FindSeriesById(request.SeriesId);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        if (series.CreatedBy != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        DateRangeUtils.UpdateDateRange(request.Current, request.Compare);

        (int seriesReviewCount, float seriesReviewRating) = await _analyticRepository.GetSeriesReviewCountAsync(
            request.Current,
            request.SeriesId
        );

        int seriesReviewRanking = await _analyticRepository.GetSeriesReviewRankingAsync(
            request.Current,
            request.SeriesId
        );

        IEnumerable<ReviewChartDataDto>? seriesReviewCountBreakdownInDay =
            await _analyticRepository.GetSeriesReviewCountBreakdownInDayAsync(
                request.Current,
                request.SeriesId
            );

        if (request.Compare is not null)
        {
            (int compareSeriesReviewCount, float compareSeriesReviewRating) =
                await _analyticRepository.GetSeriesReviewCountAsync(
                    request.Compare,
                    request.SeriesId
                );

            int compareSeriesReviewRanking = await _analyticRepository.GetSeriesReviewRankingAsync(
                request.Compare,
                request.SeriesId
            );

            List<ReviewChartDataDto>? compareSeriesReviewCountBreakdownInDay =
                (await _analyticRepository.GetSeriesReviewCountBreakdownInDayAsync(
                    request.Compare,
                    request.SeriesId
                )).ToList();

            return new ReviewAnalyticsVm(
                seriesReviewCount,
                compareSeriesReviewCount,
                seriesReviewRating,
                compareSeriesReviewRating,
                seriesReviewRanking,
                compareSeriesReviewRanking,
                seriesReviewCountBreakdownInDay.Select((x, i) => new ReviewChartData(
                    x.Time,
                    x.Positive,
                    x.Negative,
                    compareSeriesReviewCountBreakdownInDay[i].Positive,
                    compareSeriesReviewCountBreakdownInDay[i].Negative
                ))
            );
        }

        return new ReviewAnalyticsVm(
            seriesReviewCount,
            null,
            seriesReviewRating,
            null,
            seriesReviewRanking,
            null,
            seriesReviewCountBreakdownInDay.Select(x => new ReviewChartData(
                x.Time,
                x.Positive,
                x.Negative,
                null,
                null
            ))
        );
    }
}

public class GetSeriesReviewAnalyticsQueryValidator : AbstractValidator<GetSeriesReviewAnalyticsQuery>
{
    public GetSeriesReviewAnalyticsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
