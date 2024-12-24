using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Administrator.SystemSettings.Commands.UpdateKanaExchangeRate;

[Authorize(Roles = Roles.Administrator)]
public sealed record UpdateKanaExchangeRateCommand(decimal Rate) : IRequest;

public sealed class UpdateKanaExchangeRateCommandHandler : IRequestHandler<UpdateKanaExchangeRateCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateKanaExchangeRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateKanaExchangeRateCommand request, CancellationToken cancellationToken)
    {
        KanaExchangeRate? exchangeRate = await _context.KanaExchangeRates.FirstAsync(cancellationToken);

        exchangeRate.UpdateRate(request.Rate);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
