using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;
using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Users.Commands.ProcessUpgradeSubscriptionTier;

public sealed record ProcessUpgradeSubscriptionTiersCommand(string OrderId) : IRequest<OrderResult>;

public sealed class ProcessUpgradeSubscriptionTiersCommandHandler
    : IRequestHandler<ProcessUpgradeSubscriptionTiersCommand, OrderResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public ProcessUpgradeSubscriptionTiersCommandHandler(
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

    public async Task<OrderResult> Handle(ProcessUpgradeSubscriptionTiersCommand request,
        CancellationToken cancellationToken)
    {
        OrderResult result = await _paymentService.CaptureOrder(request.OrderId);

        if (result.StatusCode == 500)
        {
            return result;
        }

        SubscriptionOrder? order = _dbContext.SubscriptionOrders
            .Include(po => po.Subscription)
            .ThenInclude(s => s.Tier)
            .ThenInclude(t => t.Series)
            .FirstOrDefault(p => p.Id == request.OrderId);

        if (order is null)
        {
            throw new NotFoundException(request.OrderId, nameof(SubscriptionOrder));
        }

        if (order.UserId != _user.Id)
        {
            throw new ForbiddenAccessException();
        }

        IDomainUser? user = await _userRepository.GetById(order.UserId);

        if (result.Data.Status.ToString() == "Completed")
        {
            order.CompleteOrder();

            Guid? upgradeTierId = order.UpgradeTierId;

            DashiFan? tier = _dbContext.DashiFans.FirstOrDefault(df => df.Id == upgradeTierId);

            if (tier is null)
            {
                throw new Exception("Failed to get upgrade tier");
            }

            order.Subscription.ChangeTier(tier);

            RevenueService? revenueService = new();

            IDomainUser? author = await _userRepository.GetById(order.Subscription.Tier.Series.CreatedBy!);

            if (author is not null)
            {
                CommissionRate? commissionRate = await _dbContext.CommissionRates
                    .FirstAsync(cr => cr.Type == CommissionType.DashiFan, cancellationToken);

                revenueService.ReceiveDashiFanRevenue(author, order.Subscription, order, commissionRate);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        if (result.Data.Status.ToString() == "Voided")
        {
            order.CancelOrder();

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}
