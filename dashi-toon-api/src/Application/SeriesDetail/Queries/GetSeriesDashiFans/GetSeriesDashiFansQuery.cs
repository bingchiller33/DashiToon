using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.SeriesDetail.Queries.GetSeriesDashiFans;

public sealed record GetSeriesDashiFansQuery(int SeriesId) : IRequest<List<SeriesDashiFanVm>>;

public sealed class GetSeriesDashiFanQueryHandler : IRequestHandler<GetSeriesDashiFansQuery, List<SeriesDashiFanVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrencyService _currencyService;

    public GetSeriesDashiFanQueryHandler(IApplicationDbContext dbContext, ICurrencyService currencyService)
    {
        _dbContext = dbContext;
        _currencyService = currencyService;
    }

    public async Task<List<SeriesDashiFanVm>> Handle(GetSeriesDashiFansQuery request,
        CancellationToken cancellationToken)
    {
        Series? series = await _dbContext.Series
            .Include(v => v.Tiers.Where(t => t.IsActive))
            .FirstOrDefaultAsync(
                s => s.Id == request.SeriesId
                     && s.Status != SeriesStatus.Trashed
                     && s.Status != SeriesStatus.Hiatus
                     && s.Status != SeriesStatus.Draft,
                cancellationToken);

        if (series is null)
        {
            return [];
        }

        return series.Tiers
            .Select(tier => new SeriesDashiFanVm(
                tier.Id,
                tier.Name,
                tier.Price,
                _currencyService.ConvertPrice(tier.Price, "USD"),
                tier.Description,
                tier.Perks))
            .OrderBy(t => t.Price.Amount)
            .ToList();
    }
}
