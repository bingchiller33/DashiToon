using DashiToon.Api.Application.Administrator.SystemSettings.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetKanaExchangeRate;

[Authorize(Roles = Roles.Administrator)]
public sealed record GetKanaExchangeRateQuery : IRequest<KanaExchangeRateVm>;

public sealed class GetKanaExchangeRateQueryHandler : IRequestHandler<GetKanaExchangeRateQuery, KanaExchangeRateVm>
{
    private readonly IApplicationDbContext _dbContext;

    public GetKanaExchangeRateQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<KanaExchangeRateVm> Handle(GetKanaExchangeRateQuery request, CancellationToken cancellationToken)
    {
        KanaExchangeRate? exchangeRate = await _dbContext.KanaExchangeRates.FirstAsync(cancellationToken);

        return new KanaExchangeRateVm(exchangeRate.TargetCurrency, exchangeRate.Rate);
    }
}
