using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanRankings;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanAnalytics;

[Authorize]
public sealed record GetSeriesDashiFanAnalyticsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<DashiFanAnalyticsVm>;

public sealed class GetSeriesDashiFanAnalyticsQueryHandler
    : IRequestHandler<GetSeriesDashiFanAnalyticsQuery, DashiFanAnalyticsVm>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUser _user;

    public GetSeriesDashiFanAnalyticsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<DashiFanAnalyticsVm> Handle(
        GetSeriesDashiFanAnalyticsQuery request,
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

        TimeSpan dateDiff = request.Current.To - request.Current.From;

        int dashifanSubscriberCount = await _analyticRepository.GetSeriesDashiFanSubscriberCountAsync(
            request.Current,
            request.SeriesId
        );

        int previousDashifanSubscriberCount = await _analyticRepository.GetSeriesDashiFanSubscriberCountAsync(
            new DateRange { From = request.Current.From - dateDiff, To = request.Current.To - dateDiff },
            request.SeriesId
        );

        double growth = 0;

        if (previousDashifanSubscriberCount != 0)
        {
            growth = Math.Round(
                (double)((dashifanSubscriberCount - previousDashifanSubscriberCount) * 100) /
                previousDashifanSubscriberCount,
                2,
                MidpointRounding.ToNegativeInfinity);
        }

        int dashifanRanking = await _analyticRepository.GetSeriesDashiFanRankingAsync(
            request.Current,
            request.SeriesId
        );

        IEnumerable<ChartDataDto>? dashifanSubscriberCountBreakdownInMonth =
            await _analyticRepository.GetSeriesDashiFanBreakdownInMonthAsync(
                request.Current,
                request.SeriesId
            );

        (int Count, IEnumerable<DashiFanRankingDto> Data) dashiFanTierRankings =
            await _analyticRepository.GetSeriesDashiFanTierRankingAsync(
                request.Current,
                request.SeriesId
            );

        if (request.Compare != null)
        {
            int compareDashifanSubscriberCount = await _analyticRepository.GetSeriesDashiFanSubscriberCountAsync(
                request.Compare,
                request.SeriesId
            );

            int previousCompareDashifanSubscriberCount =
                await _analyticRepository.GetSeriesDashiFanSubscriberCountAsync(
                    new DateRange { From = request.Compare.From - dateDiff, To = request.Compare.To - dateDiff },
                    request.SeriesId
                );

            double compareGrowth = 0;

            if (previousDashifanSubscriberCount != 0)
            {
                compareGrowth = Math.Round(
                    (double)((compareDashifanSubscriberCount - previousCompareDashifanSubscriberCount) * 100) /
                    previousCompareDashifanSubscriberCount,
                    2,
                    MidpointRounding.ToNegativeInfinity);
            }

            int compareDashifanRanking = await _analyticRepository.GetSeriesDashiFanRankingAsync(
                request.Compare,
                request.SeriesId
            );

            List<ChartDataDto>? compareDashifanSubscriberCountBreakdownInMonth =
                (await _analyticRepository.GetSeriesDashiFanBreakdownInMonthAsync(
                    request.Compare,
                    request.SeriesId
                )).ToList();

            List<DashiFanRankingDto>? compareDashiFanTierRankings =
                (await _analyticRepository.GetSeriesDashiFanTierRankingAsync(
                    request.Compare,
                    request.SeriesId
                )).Data.ToList();

            return new DashiFanAnalyticsVm(
                dashifanSubscriberCount,
                compareDashifanSubscriberCount,
                growth,
                compareGrowth,
                dashifanRanking,
                compareDashifanRanking,
                dashifanSubscriberCountBreakdownInMonth.Select((x, i) => new ChartData(
                    x.Time,
                    x.Data,
                    compareDashifanSubscriberCountBreakdownInMonth[i].Data
                )),
                dashiFanTierRankings.Data.Select((x, i) => new DashiFanRanking(
                    x.Id,
                    x.Name,
                    x.Data,
                    compareDashiFanTierRankings[i].Data,
                    x.Rank
                ))
            );
        }

        return new DashiFanAnalyticsVm(
            dashifanSubscriberCount,
            null,
            growth,
            null,
            dashifanRanking,
            null,
            dashifanSubscriberCountBreakdownInMonth.Select(x => new ChartData(
                x.Time,
                x.Data,
                null
            )),
            dashiFanTierRankings.Data.Select(x => new DashiFanRanking(
                x.Id,
                x.Name,
                x.Data,
                null,
                x.Rank
            ))
        );
    }
}

public class GetSeriesDashiFanAnalyticsQueryValidator : AbstractValidator<GetSeriesDashiFanAnalyticsQuery>
{
    public GetSeriesDashiFanAnalyticsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
