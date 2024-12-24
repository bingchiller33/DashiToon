using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetChapterViewRankings;

[Authorize]
public sealed record GetChapterViewRankingsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId,
    int PageNumber = 1,
    int PageSize = 5,
    string Query = "",
    bool Desc = true
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<PaginatedList<ChapterRankings>>;

public sealed class GetChapterRankingsQueryHandler
    : IRequestHandler<GetChapterViewRankingsQuery, PaginatedList<ChapterRankings>>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUser _user;

    public GetChapterRankingsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<PaginatedList<ChapterRankings>> Handle(
        GetChapterViewRankingsQuery request,
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

        (int count, IEnumerable<ChapterRankingsDto> seriesChapterViewRankings) =
            await _analyticRepository.GetSeriesChapterViewRankingAsync(
                request.Current,
                request.SeriesId,
                request.Query,
                request.Desc,
                request.PageNumber,
                request.PageSize
            );

        if (request.Compare is not null)
        {
            List<ChapterRankingsDto>? compareSeriesChapterViewRankings =
                (await _analyticRepository.GetSeriesChapterViewRankingAsync(
                    request.Compare,
                    request.SeriesId,
                    request.Query,
                    request.Desc,
                    request.PageNumber,
                    request.PageSize
                )).Data.ToList();

            return new PaginatedList<ChapterRankings>(
                seriesChapterViewRankings.Select((x, i) => new ChapterRankings(
                    x.Id,
                    x.ChapterNumber,
                    x.Name,
                    x.VolumeId,
                    x.VolumeName,
                    x.Data,
                    compareSeriesChapterViewRankings[i].Data,
                    x.Rank
                )).ToList(),
                count,
                request.PageNumber,
                request.PageSize);
        }

        return new PaginatedList<ChapterRankings>(
            seriesChapterViewRankings.Select(x => new ChapterRankings(
                x.Id,
                x.ChapterNumber,
                x.Name,
                x.VolumeId,
                x.VolumeName,
                x.Data,
                null,
                x.Rank
            )).ToList(),
            count,
            request.PageNumber,
            request.PageSize);
    }
}

public class GetChapterViewRankingsQueryValidator : AbstractValidator<GetChapterViewRankingsQuery>
{
    public GetChapterViewRankingsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
