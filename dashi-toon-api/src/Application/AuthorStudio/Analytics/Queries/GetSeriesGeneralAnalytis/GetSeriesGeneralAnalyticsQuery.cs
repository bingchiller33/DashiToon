using DashiToon.Api.Application.AuthorStudio.Analytics.Models;
using DashiToon.Api.Application.AuthorStudio.Analytics.Validators;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.AuthorStudio.Analytics.Queries.GetSeriesGeneralAnalytis;

[Authorize]
public sealed record GetSeriesGeneralAnalyticsQuery(
    DateRange Current,
    DateRange? Compare,
    int SeriesId
) : BaseAnalyticsQuery(Current, Compare, SeriesId), IRequest<GeneralAnalyticsVm>;

public sealed class GetSeriesGeneralAnalyticsQueryHandler
    : IRequestHandler<GetSeriesGeneralAnalyticsQuery, GeneralAnalyticsVm>
{
    private readonly IAnalyticRepository _analyticRepository;
    private readonly ISeriesRepository _seriesRepository;
    private readonly IUser _user;

    public GetSeriesGeneralAnalyticsQueryHandler(
        IAnalyticRepository analyticRepository,
        ISeriesRepository seriesRepository,
        IUser user)
    {
        _analyticRepository = analyticRepository;
        _seriesRepository = seriesRepository;
        _user = user;
    }

    public async Task<GeneralAnalyticsVm> Handle(GetSeriesGeneralAnalyticsQuery request,
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

        if (request.Compare is not null)
        {
            GeneralAnalyticsDto? currentGeneralAnalytics = await _analyticRepository.GetGeneralAnalyticsAsync(
                request.Current,
                request.SeriesId);

            GeneralAnalyticsDto? compareGeneralAnalytics = await _analyticRepository.GetGeneralAnalyticsAsync(
                request.Compare,
                request.SeriesId);

            return new GeneralAnalyticsVm(
                currentGeneralAnalytics.ViewCount,
                compareGeneralAnalytics.ViewCount,
                currentGeneralAnalytics.Rating,
                compareGeneralAnalytics.Rating,
                currentGeneralAnalytics.Revenue,
                compareGeneralAnalytics.Revenue,
                currentGeneralAnalytics.DashiFanCount,
                compareGeneralAnalytics.DashiFanCount
            );
        }

        GeneralAnalyticsDto? generalAnalytics = await _analyticRepository.GetGeneralAnalyticsAsync(
            request.Current,
            request.SeriesId);

        return new GeneralAnalyticsVm(
            generalAnalytics.ViewCount,
            null,
            generalAnalytics.Rating,
            null,
            generalAnalytics.Revenue,
            null,
            generalAnalytics.DashiFanCount,
            null
        );
    }
}

public class GetSeriesGeneralAnalyticsQueryValidator : AbstractValidator<GetSeriesGeneralAnalyticsQuery>
{
    public GetSeriesGeneralAnalyticsQueryValidator()
    {
        Include(new BaseAnalyticsQueryValidator());
    }
}
