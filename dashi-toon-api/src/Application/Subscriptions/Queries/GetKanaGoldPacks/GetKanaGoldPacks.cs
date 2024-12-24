using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.Subscriptions.Queries.GetKanaGoldPacks;

[Authorize]
public sealed record GetKanaGoldPacks : IRequest<List<KanaGoldPackVm>>;

public sealed class GetKanaGoldPacksHandler : IRequestHandler<GetKanaGoldPacks, List<KanaGoldPackVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrencyService _currencyService;

    public GetKanaGoldPacksHandler(IApplicationDbContext dbContext, ICurrencyService currencyService)
    {
        _dbContext = dbContext;
        _currencyService = currencyService;
    }

    public async Task<List<KanaGoldPackVm>> Handle(GetKanaGoldPacks request, CancellationToken cancellationToken)
    {
        List<KanaGoldPackVm>? result = await _dbContext.KanaGoldPacks
            .AsNoTracking()
            .Where(x => x.IsActive == true)
            .Select(x => new KanaGoldPackVm(x.Id, x.Amount, x.Price))
            .ToListAsync(cancellationToken);

        result.ForEach(x => x.Price = _currencyService.ConvertPrice(x.Price, "VND"));
        return result;
    }
}
