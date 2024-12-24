using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;
using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Users.Commands.UpgradeSubscriptionTier;

[Authorize]
public sealed record UpgradeSubscriptionTiersCommand(Guid SubscriptionId, Guid TierId) : IRequest<OrderResult>;

public sealed class UpgradeSubscriptionTiersCommandHandler
    : IRequestHandler<UpgradeSubscriptionTiersCommand, OrderResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public UpgradeSubscriptionTiersCommandHandler(
        IApplicationDbContext dbContext,
        IPaymentService paymentService,
        IUserRepository userRepository,
        IUser user)
    {
        _dbContext = dbContext;
        _paymentService = paymentService;
        _userRepository = userRepository;
        _user = user;
    }

    public async Task<OrderResult> Handle(UpgradeSubscriptionTiersCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        Subscription? subscription = user.Subscriptions.FirstOrDefault(s => s.Id == request.SubscriptionId);

        if (subscription is null)
        {
            throw new ForbiddenAccessException();
        }

        DashiFan? tier = _dbContext.DashiFans
            .FirstOrDefault(df => df.Id == request.TierId
                                  && df.SeriesId == subscription.Tier.SeriesId
                                  && df.IsActive);

        if (tier is null)
        {
            throw new NotFoundException(request.TierId.ToString(), nameof(DashiFan));
        }

        Price priceDifference = SubscriptionService.UpgradeTier(subscription, tier);

        OrderResult result = await _paymentService.CreateOrder(priceDifference);

        if (result.StatusCode == 500)
        {
            throw new Exception("Failed to change tier");
        }

        SubscriptionOrder? order = SubscriptionOrder.CreateUpgradeOrder(
            (string)result.Data.Id,
            user.Id,
            subscription.Id,
            tier.Id,
            priceDifference);

        _dbContext.SubscriptionOrders.Add(order);

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
