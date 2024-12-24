using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesKanaAnalytics;

[Authorize]
public sealed record GetSeriesKanaAnalyticsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<KanaAnalyticsVm>;

public sealed class GetSeriesKanaAnalyticsQueryHandler : IRequestHandler<GetSeriesKanaAnalyticsQuery, KanaAnalyticsVm>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUser _user;

    public GetSeriesKanaAnalyticsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<KanaAnalyticsVm> Handle(GetSeriesKanaAnalyticsQuery request, CancellationToken cancellationToken)
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

        int seriesKanaRevenue = await _analyticRepository.GetSeriesKanaRevenueAsync(
            request.Current,
            request.SeriesId
        );

        int previousSeriesKanaRevenue = await _analyticRepository.GetSeriesKanaRevenueAsync(
            new DateRange { From = request.Current.From - dateDiff, To = request.Current.To - dateDiff },
            request.SeriesId
        );

        double growth = 0;

        if (previousSeriesKanaRevenue != 0)
        {
            growth = Math.Round(
                (double)((seriesKanaRevenue - previousSeriesKanaRevenue) * 100) /
                previousSeriesKanaRevenue,
                2,
                MidpointRounding.ToNegativeInfinity);
        }

        int seriesKanaRevenueRanking = await _analyticRepository.GetSeriesKanaRevenueRankingAsync(
            request.Current,
            request.SeriesId
        );

        IEnumerable<ChartDataDto>? seriesKanaRevenueBreakdownInDay =
            await _analyticRepository.GetSeriesKanaRevenueBreakdownInDayAsync(
                request.Current,
                request.SeriesId
            );

        IEnumerable<ChapterRankingsDto>? seriesChapterKanaRevenueRankings =
            (await _analyticRepository.GetSeriesChapterKanaRevenueRankingAsync(
                request.Current,
                request.SeriesId
            )).Data;

        if (request.Compare is not null)
        {
            int compareSeriesKanaRevenue = await _analyticRepository.GetSeriesKanaRevenueAsync(
                request.Compare,
                request.SeriesId
            );

            int compareSeriesKanaRevenueRanking = await _analyticRepository.GetSeriesKanaRevenueRankingAsync(
                request.Compare,
                request.SeriesId
            );

            int previousCompareSeriesKanaRevenue = await _analyticRepository.GetSeriesKanaRevenueAsync(
                new DateRange { From = request.Compare.From - dateDiff, To = request.Compare.To - dateDiff },
                request.SeriesId
            );

            double compareGrowth = 0;

            if (previousCompareSeriesKanaRevenue != 0)
            {
                compareGrowth = Math.Round(
                    (float)((compareSeriesKanaRevenueRanking - previousCompareSeriesKanaRevenue) * 100) /
                    previousCompareSeriesKanaRevenue,
                    2,
                    MidpointRounding.ToNegativeInfinity);
            }

            List<ChartDataDto>? compareSeriesKanaRevenueBreakdownInDay =
                (await _analyticRepository.GetSeriesKanaRevenueBreakdownInDayAsync(
                    request.Compare,
                    request.SeriesId
                )).ToList();

            List<ChapterRankingsDto>? compareSeriesChapterKanaRevenueRankings =
                (await _analyticRepository.GetSeriesChapterKanaRevenueRankingAsync(
                    request.Compare,
                    request.SeriesId
                )).Data.ToList();

            return new KanaAnalyticsVm(
                seriesKanaRevenue,
                compareSeriesKanaRevenue,
                growth,
                compareGrowth,
                seriesKanaRevenueRanking,
                compareSeriesKanaRevenueRanking,
                seriesKanaRevenueBreakdownInDay.Select((x, i) => new ChartData(
                    x.Time,
                    x.Data,
                    compareSeriesKanaRevenueBreakdownInDay[i].Data
                )),
                seriesChapterKanaRevenueRankings.Select((x, i) => new ChapterRankings(
                    x.Id,
                    x.ChapterNumber,
                    x.Name,
                    x.VolumeId,
                    x.VolumeName,
                    x.Data,
                    compareSeriesChapterKanaRevenueRankings[i].Data,
                    x.Rank
                ))
            );
        }

        return new KanaAnalyticsVm(
            seriesKanaRevenue,
            null,
            growth,
            null,
            seriesKanaRevenueRanking,
            null,
            seriesKanaRevenueBreakdownInDay.Select(x => new ChartData(
                x.Time,
                x.Data,
                null
            )),
            seriesChapterKanaRevenueRankings.Select(x => new ChapterRankings(
                x.Id,
                x.ChapterNumber,
                x.Name,
                x.VolumeId,
                x.VolumeName,
                x.Data,
                null,
                x.Rank
            ))
        );
    }
}

public class GetSeriesKanaAnalyticsQueryValidator : AbstractValidator<GetSeriesKanaAnalyticsQuery>
{
    public GetSeriesKanaAnalyticsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
