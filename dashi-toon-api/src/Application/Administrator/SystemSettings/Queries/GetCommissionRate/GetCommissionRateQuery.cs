using DashiToon.Api.Application.Administrator.SystemSettings.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Administrator.SystemSettings.Queries.GetCommissionRate;

[Authorize(Roles = Roles.Administrator)]
public sealed record GetCommissionRateQuery(CommissionType Type) : IRequest<CommissionRateVm>;

public sealed class GetCommissionRateQueryHandler
    : IRequestHandler<GetCommissionRateQuery, CommissionRateVm>
{
    private readonly IApplicationDbContext _dbContext;

    public GetCommissionRateQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommissionRateVm> Handle(GetCommissionRateQuery request,
        CancellationToken cancellationToken)
    {
        CommissionRate? commissionRate = await _dbContext.CommissionRates
            .FirstAsync(cr => cr.Type == request.Type, cancellationToken);

        return new CommissionRateVm(
            commissionRate.Type,
            commissionRate.RatePercentage
        );
    }
}
