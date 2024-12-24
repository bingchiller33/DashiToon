using DashiToon.Api.Application.Administrator.KanaGoldPacks.Models;
using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Constants;
using DashiToon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DashiToon.Api.Application.Administrator.KanaGoldPacks.Commands.UpdateKanaGoldPackStatus;

[Authorize(Roles = Roles.Administrator)]
public sealed record UpdateKanaGoldPackStatusCommand(Guid Id, bool IsActive) : IRequest<KanaGoldPackVm>;

public sealed class UpdateKanaGoldPackStatusCommandHandler
    : IRequestHandler<UpdateKanaGoldPackStatusCommand, KanaGoldPackVm>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateKanaGoldPackStatusCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<KanaGoldPackVm> Handle(
        UpdateKanaGoldPackStatusCommand request,
        CancellationToken cancellationToken)
    {
        KanaGoldPack? kanaGoldPack = await _dbContext.KanaGoldPacks
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (kanaGoldPack is null)
        {
            throw new NotFoundException(request.Id.ToString(), nameof(KanaGoldPack));
        }

        kanaGoldPack.UpdateStatus(request.IsActive);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new KanaGoldPackVm(
            kanaGoldPack.Id,
            kanaGoldPack.Amount,
            kanaGoldPack.Price,
            kanaGoldPack.IsActive
        );
    }
}
