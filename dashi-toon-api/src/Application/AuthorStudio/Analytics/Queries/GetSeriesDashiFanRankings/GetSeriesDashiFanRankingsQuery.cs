using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesDashiFanRankings;

[Authorize]
public sealed record GetSeriesDashiFanRankingsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId,
    int PageNumber = 1,
    int PageSize = 5,
    string Query = "",
    bool Desc = true
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<PaginatedList<DashiFanRanking>>;

public sealed class GetSeriesDashiFanRankingsQueryHandler
    : IRequestHandler<GetSeriesDashiFanRankingsQuery, PaginatedList<DashiFanRanking>>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUser _user;

    public GetSeriesDashiFanRankingsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<PaginatedList<DashiFanRanking>> Handle(
        GetSeriesDashiFanRankingsQuery request,
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

        (int count, IEnumerable<DashiFanRankingDto> dashiFanTierRankings) =
            await _analyticRepository.GetSeriesDashiFanTierRankingAsync(
                request.Current,
                request.SeriesId,
                request.Query,
                request.Desc,
                request.PageNumber,
                request.PageSize
            );

        if (request.Compare is not null)
        {
            List<DashiFanRankingDto>? compareDashiFanTierRankings =
                (await _analyticRepository.GetSeriesDashiFanTierRankingAsync(
                    request.Compare,
                    request.SeriesId,
                    request.Query,
                    request.Desc,
                    request.PageNumber,
                    request.PageSize
                )).Data.ToList();

            return new PaginatedList<DashiFanRanking>(
                dashiFanTierRankings.Select((x, i) => new DashiFanRanking(
                    x.Id,
                    x.Name,
                    x.Data,
                    compareDashiFanTierRankings[i].Data,
                    x.Rank
                )).ToList(),
                count,
                request.PageNumber,
                request.PageSize);
        }

        return new PaginatedList<DashiFanRanking>(
            dashiFanTierRankings.Select(x => new DashiFanRanking(
                x.Id,
                x.Name,
                x.Data,
                null,
                x.Rank
            )).ToList(),
            count,
            request.PageNumber,
            request.PageSize);
    }
}

public class GetSeriesDashiFanRankingsQueryValidator : AbstractValidator<GetSeriesDashiFanRankingsQuery>
{
    public GetSeriesDashiFanRankingsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
