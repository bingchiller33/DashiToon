using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Subscriptions.Commands.PurchaseKanaGoldPack;

[Authorize]
public sealed record PurchaseKanaGoldPackCommand(Guid PackId)
    : IRequest<OrderResult>;

public sealed class PurchaseKanaGoldPackCommandHandler : IRequestHandler<PurchaseKanaGoldPackCommand, OrderResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public PurchaseKanaGoldPackCommandHandler(
        IApplicationDbContext context,
        IUserRepository userRepository,
        IPaymentService paymentService,
        IUser user)
    {
        _context = context;
        _userRepository = userRepository;
        _paymentService = paymentService;
        _user = user;
    }

    public async Task<OrderResult> Handle(PurchaseKanaGoldPackCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        KanaGoldPack? pack = await _context.KanaGoldPacks
            .FirstOrDefaultAsync(p => p.Id == request.PackId, cancellationToken);

        if (pack is null)
        {
            throw new NotFoundException(request.PackId.ToString(), nameof(KanaGoldPack));
        }

        OrderResult result = await _paymentService.CreateOrder(pack.Price);

        if (result.StatusCode == 500)
        {
            return result;
        }

        dynamic? order = KanaService.CreatePurchaseOrder(result.Data.Id, user, pack);

        _context.PurchaseOrders.Add(order);

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}
