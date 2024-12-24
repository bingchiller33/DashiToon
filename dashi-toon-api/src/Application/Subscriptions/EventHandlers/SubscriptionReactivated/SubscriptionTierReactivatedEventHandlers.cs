using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Subscriptions.EventHandlers.SubscriptionReactivated;

public sealed class SubscriptionTierReactivatedEventHandlers : INotificationHandler<SubscriptionReactivatedEvent>
{
    private readonly IPaymentService _paymentService;

    public SubscriptionTierReactivatedEventHandlers(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Handle(SubscriptionReactivatedEvent notification, CancellationToken cancellationToken)
    {
        bool result = await _paymentService.ReactivateSubscription(
            notification.Subscription.SubscriptionId,
            notification.Reason);

        if (!result)
        {
            throw new Exception("Failed to reactivate subscription");
        }
    }
}
