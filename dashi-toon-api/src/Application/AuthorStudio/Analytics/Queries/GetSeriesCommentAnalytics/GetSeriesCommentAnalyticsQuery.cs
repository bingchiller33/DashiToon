using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesCommentAnalytics;

[Authorize]
public sealed record GetSeriesCommentAnalyticsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<CommentAnalyticsVm>;

public sealed class GetSeriesCommentAnalyticsQueryHandler
    : IRequestHandler<GetSeriesCommentAnalyticsQuery, CommentAnalyticsVm>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUser _user;

    public GetSeriesCommentAnalyticsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<CommentAnalyticsVm> Handle(
        GetSeriesCommentAnalyticsQuery request,
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

        int seriesCommentCount = await _analyticRepository.GetSeriesCommentCountAsync(
            request.Current,
            request.SeriesId);

        int previousSeriesCommentCount = await _analyticRepository.GetSeriesCommentCountAsync(
            new DateRange { From = request.Current.From - dateDiff, To = request.Current.To - dateDiff },
            request.SeriesId);

        double growth = 0;

        if (previousSeriesCommentCount != 0)
        {
            growth = Math.Round(
                (double)((seriesCommentCount - previousSeriesCommentCount) * 100) /
                previousSeriesCommentCount,
                2,
                MidpointRounding.ToNegativeInfinity);
        }

        int seriesCommentRanking = await _analyticRepository.GetSeriesCommentRankingAsync(
            request.Current,
            request.SeriesId
        );

        IEnumerable<ChartDataDto>? seriesCommentCountBreakdownInDay =
            await _analyticRepository.GetSeriesCommentCountBreakdownInDayAsync(
                request.Current,
                request.SeriesId
            );

        (int _, IEnumerable<ChapterRankingsDto> seriesChapterCommentRanking) =
            await _analyticRepository.GetSeriesChapterCommentRankingAsync(
                request.Current,
                request.SeriesId
            );

        if (request.Compare is not null)
        {
            int compareSeriesCommentCount = await _analyticRepository.GetSeriesCommentCountAsync(
                request.Compare,
                request.SeriesId);

            int previousCompareSeriesCommentCount = await _analyticRepository.GetSeriesCommentCountAsync(
                new DateRange { From = request.Compare.From - dateDiff, To = request.Compare.To - dateDiff },
                request.SeriesId);

            double compareGrowth = 0;

            if (previousCompareSeriesCommentCount != 0)
            {
                compareGrowth = Math.Round(
                    (double)((compareSeriesCommentCount - previousCompareSeriesCommentCount) * 100) /
                    previousCompareSeriesCommentCount,
                    2,
                    MidpointRounding.ToNegativeInfinity);
            }

            int compareSeriesCommentRanking = await _analyticRepository.GetSeriesCommentRankingAsync(
                request.Compare,
                request.SeriesId
            );

            List<ChartDataDto>? compareSeriesCommentCountBreakdownInDay =
                (await _analyticRepository.GetSeriesCommentCountBreakdownInDayAsync(
                    request.Compare,
                    request.SeriesId
                )).ToList();

            List<ChapterRankingsDto>? compareSeriesChapterCommentRanking =
                (await _analyticRepository.GetSeriesChapterCommentRankingAsync(
                    request.Compare,
                    request.SeriesId
                )).Data.ToList();

            return new CommentAnalyticsVm(
                seriesCommentCount,
                compareSeriesCommentCount,
                growth,
                compareGrowth,
                seriesCommentRanking,
                compareSeriesCommentRanking,
                seriesCommentCountBreakdownInDay.Select((x, i) => new ChartData(
                    x.Time,
                    x.Data,
                    compareSeriesCommentCountBreakdownInDay[i].Data
                )),
                seriesChapterCommentRanking.Select((x, i) => new ChapterRankings(
                    x.Id,
                    x.ChapterNumber,
                    x.Name,
                    x.VolumeId,
                    x.VolumeName,
                    x.Data,
                    compareSeriesChapterCommentRanking[i].Data,
                    x.Rank
                ))
            );
        }

        return new CommentAnalyticsVm(
            seriesCommentCount,
            null,
            growth,
            null,
            seriesCommentRanking,
            null,
            seriesCommentCountBreakdownInDay.Select(x => new ChartData(
                x.Time,
                x.Data,
                null
            )),
            seriesChapterCommentRanking.Select(x => new ChapterRankings(
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

public class GetSeriesCommentAnalyticsQueryValidator : AbstractValidator<GetSeriesCommentAnalyticsQuery>
{
    public GetSeriesCommentAnalyticsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
