using DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.UpdateKanaGoldPack;

[Authorize(Roles = Roles.Administrator)]
public sealed record UpdateKanaGoldPackCommand(
    Guid Id,
    int Amount,
    decimal PriceAmount,
    string Currency,
    bool IsActive) : IRequest<KanaGoldPackVm>;

public sealed class UpdateKanaGoldPackCommandHandler
    : IRequestHandler<UpdateKanaGoldPackCommand, KanaGoldPackVm>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateKanaGoldPackCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<KanaGoldPackVm> Handle(
        UpdateKanaGoldPackCommand request,
        CancellationToken cancellationToken)
    {
        KanaGoldPack? kanaGoldPack = await _dbContext.KanaGoldPacks
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (kanaGoldPack is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(KanaGoldPack));
        }

        KanaGoldPack? existingPack = await _dbContext.KanaGoldPacks
            .FirstOrDefaultAsync(x =>
                    x.Id != request.Id &&
                    x.Amount == request.Amount &&
                    x.Price.Currency == request.Currency,
                cancellationToken);

        if (existingPack != null)
        {
            throw new ArgumentException(
                $"Another KanaGoldPack with amount {request.Amount} and currency {request.Currency} already exists"
            );
        }

        Price? price = Price.CreateNew(request.PriceAmount, request.Currency);

        kanaGoldPack.Update(
            request.Amount,
            price,
            request.IsActive
        );

        _dbContext.KanaGoldPacks.Update(kanaGoldPack);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new KanaGoldPackVm(
            kanaGoldPack.Id,
            kanaGoldPack.Amount,
            kanaGoldPack.Price,
            kanaGoldPack.IsActive
        );
    }
}
