using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Subscriptions.Commands.ProcessKanaGoldPackPurchase;

public sealed record ProcessKanaGoldPackPurchaseCommand(string OrderId) : IRequest<OrderResult>;

[Authorize]
public sealed class ProcessKanaGoldPackPurchaseCommandHandler
    : IRequestHandler<ProcessKanaGoldPackPurchaseCommand, OrderResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public ProcessKanaGoldPackPurchaseCommandHandler(
        IPaymentService paymentService,
        IUserRepository userRepository,
        IApplicationDbContext dbContext,
        IUser user)
    {
        _paymentService = paymentService;
        _userRepository = userRepository;
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<OrderResult> Handle(
        ProcessKanaGoldPackPurchaseCommand request,
        CancellationToken cancellationToken)
    {
        OrderResult result = await _paymentService.CaptureOrder(request.OrderId);

        if (result.StatusCode == 500)
        {
            return result;
        }

        PurchaseOrder? order = _dbContext.PurchaseOrders
            .Include(po => po.KanaGoldPack)
            .FirstOrDefault(p => p.Id == request.OrderId);

        if (order is null)
        {
            throw new NotFoundException(request.OrderId, nameof(PurchaseOrder));
        }

        if (order.UserId != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        IDomainUser? user = await _userRepository.GetById(order.UserId);

        if (result.Data.Status.ToString() == "Completed")
        {
            KanaService.CompleteOrder(user!, order, order.KanaGoldPack);

            await _userRepository.Update(user!);
        }

        if (result.Data.Status.ToString() == "Voided")
        {
            order.CancelOrder();

            await _userRepository.Update(user!);
        }

        return result;
    }
}
