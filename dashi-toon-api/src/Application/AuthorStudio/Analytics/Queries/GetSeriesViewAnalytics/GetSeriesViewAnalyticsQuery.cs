using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesViewAnalytics;

[Authorize]
public sealed record GetSeriesViewAnalyticsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<ViewAnalyticsVm>;

public sealed class GetSeriesViewAnalyticsQueryHandler : IRequestHandler<GetSeriesViewAnalyticsQuery, ViewAnalyticsVm>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;

    private readonly IUser _user;

    private static readonly IDictionary<string, string> MapWeekDays = new Dictionary<string, string>
    {
        { "Sunday", "Chủ Nhật" },
        { "Monday", "Thứ 2" },
        { "Tuesday", "Thứ 3" },
        { "Wednesday", "Thứ 4" },
        { "Thursday", "Thứ 5" },
        { "Friday", "Thứ 6" },
        { "Saturday", "Thứ 7" }
    };

    public GetSeriesViewAnalyticsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<ViewAnalyticsVm> Handle(GetSeriesViewAnalyticsQuery request, CancellationToken cancellationToken)
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

        int seriesViewRanking = await _analyticRepository.GetSeriesViewRankingAsync(request.Current, request.SeriesId);

        int seriesViewCount = await _analyticRepository.GetSeriesViewCountAsync(request.Current, request.SeriesId);

        int previousSeriesViewCount = await _analyticRepository.GetSeriesViewCountAsync(
            new DateRange { From = request.Current.From - dateDiff, To = request.Current.To - dateDiff },
            request.SeriesId);

        double growth = 0;

        if (previousSeriesViewCount != 0)
        {
            growth = Math.Round(
                (double)((seriesViewCount - previousSeriesViewCount) * 100) /
                previousSeriesViewCount,
                2,
                MidpointRounding.ToNegativeInfinity);
        }

        List<ChartDataDto>? seriesViewCountBreakdownInWeek =
            (await _analyticRepository.GetSeriesViewCountBreakdownInWeekAsync(
                request.Current,
                request.SeriesId)).ToList();

        List<ChartDataDto>? seriesViewCountBreakdownInDay =
            (await _analyticRepository.GetSeriesViewCountBreakdownInDayAsync(
                request.Current,
                request.SeriesId)).ToList();

        (int Count, IEnumerable<ChapterRankingsDto> Data) seriesChapterViewRankings =
            await _analyticRepository.GetSeriesChapterViewRankingAsync(
                request.Current,
                request.SeriesId);

        if (request.Compare is not null)
        {
            int compareSeriesRanking = await _analyticRepository.GetSeriesViewRankingAsync(
                request.Compare,
                request.SeriesId);

            int compareSeriesViewCount = await _analyticRepository.GetSeriesViewCountAsync(
                request.Compare,
                request.SeriesId);

            int previousCompareSeriesViewCount = await _analyticRepository.GetSeriesViewCountAsync(
                new DateRange { From = request.Compare.From - dateDiff, To = request.Compare.To - dateDiff },
                request.SeriesId);

            double compareGrowth = 0;

            if (previousSeriesViewCount != 0)
            {
                compareGrowth = Math.Round(
                    (double)((compareSeriesViewCount - previousCompareSeriesViewCount) * 100) /
                    previousCompareSeriesViewCount,
                    2,
                    MidpointRounding.ToNegativeInfinity);
            }

            List<ChartDataDto>? compareSeriesViewCountBreakdownInWeek =
                (await _analyticRepository.GetSeriesViewCountBreakdownInWeekAsync(
                    request.Compare,
                    request.SeriesId)).ToList();

            List<ChartDataDto>? compareSeriesViewCountBreakdownInDay =
                (await _analyticRepository.GetSeriesViewCountBreakdownInDayAsync(
                    request.Compare,
                    request.SeriesId)).ToList();

            List<ChapterRankingsDto>? compareSeriesChapterViewRankings =
                (await _analyticRepository.GetSeriesChapterViewRankingAsync(
                    request.Compare,
                    request.SeriesId)).Data.ToList();

            return new ViewAnalyticsVm(
                seriesViewCount,
                compareSeriesViewCount,
                growth,
                compareGrowth,
                seriesViewRanking,
                compareSeriesRanking,
                seriesViewCountBreakdownInDay.Select((x, i) => new ChartData(
                    x.Time,
                    x.Data,
                    compareSeriesViewCountBreakdownInDay[i].Data
                )).ToList(),
                seriesChapterViewRankings.Data.Select((x, i) => new ChapterRankings(
                    x.Id,
                    x.ChapterNumber,
                    x.Name,
                    x.VolumeId,
                    x.VolumeName,
                    x.Data,
                    compareSeriesChapterViewRankings[i].Data,
                    x.Rank
                )),
                MapWeekDays[seriesViewCountBreakdownInWeek.OrderByDescending(x => x.Data).First().Time],
                MapWeekDays[seriesViewCountBreakdownInWeek.OrderBy(x => x.Data).First().Time],
                seriesViewCountBreakdownInWeek.Select((x, i) => new ChartData(
                    MapWeekDays[x.Time],
                    x.Data,
                    compareSeriesViewCountBreakdownInWeek[i].Data
                ))
            );
        }

        return new ViewAnalyticsVm(
            seriesViewCount,
            null,
            growth,
            null,
            seriesViewRanking,
            null,
            seriesViewCountBreakdownInDay.Select(x => new ChartData(
                x.Time,
                x.Data,
                null
            )).ToList(),
            seriesChapterViewRankings.Data.Select(x => new ChapterRankings(
                x.Id,
                x.ChapterNumber,
                x.Name,
                x.VolumeId,
                x.VolumeName,
                x.Data,
                null,
                x.Rank
            )),
            MapWeekDays[seriesViewCountBreakdownInWeek.OrderByDescending(x => x.Data).First().Time],
            MapWeekDays[seriesViewCountBreakdownInWeek.OrderBy(x => x.Data).First().Time],
            seriesViewCountBreakdownInWeek.Select(x => new ChartData(
                MapWeekDays[x.Time],
                x.Data,
                null))
        );
    }
}

public class GetSeriesViewAnalyticsQueryValidator : AbstractValidator<GetSeriesViewAnalyticsQuery>
{
    public GetSeriesViewAnalyticsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
