using DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.ValueObjects;
using DashiToon.Api.Application.Common.Exceptions;

namespace DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.CreateKanaGoldPack;

[Authorize(Roles = Roles.Administrator)]
public sealed record CreateKanaGoldPackCommand(int Amount, decimal PriceAmount, string Currency)
    : IRequest<KanaGoldPackVm>;

public sealed class CreateKanaGoldPackCommandHandler
    : IRequestHandler<CreateKanaGoldPackCommand, KanaGoldPackVm>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateKanaGoldPackCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<KanaGoldPackVm> Handle(CreateKanaGoldPackCommand request, CancellationToken cancellationToken)
    {
        KanaGoldPack? existingPack = await _dbContext.KanaGoldPacks
            .FirstOrDefaultAsync(x => x.Amount == request.Amount
                                      && x.Price.Currency == request.Currency,
                cancellationToken);

        if (existingPack != null)
        {
            throw new ArgumentException(
                $"A KanaGoldPack with amount {request.Amount} and currency {request.Currency} already exists");
        }

        Price? price = Price.CreateNew(request.PriceAmount, request.Currency);

        KanaGoldPack? kanaGoldPack = KanaGoldPack.Create(
            request.Amount,
            price,
            true
        );

        _dbContext.KanaGoldPacks.Add(kanaGoldPack);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new KanaGoldPackVm(
            kanaGoldPack.Id,
            kanaGoldPack.Amount,
            kanaGoldPack.Price,
            kanaGoldPack.IsActive
        );
    }
}
