using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Administrator.SystemSettings.Commands.UpdateCommissionRate;

[Authorize(Roles = Roles.Administrator)]
public sealed record UpdateCommissionRateCommand(CommissionType Type, decimal Rate) : IRequest;

public sealed class UpdateCommissionRateCommandHandler : IRequestHandler<UpdateCommissionRateCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCommissionRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCommissionRateCommand request, CancellationToken cancellationToken)
    {
        CommissionRate? commissionRate = await _context.CommissionRates
            .FirstAsync(cr => cr.Type == request.Type, cancellationToken);

        commissionRate.UpdateRate(request.Rate);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
