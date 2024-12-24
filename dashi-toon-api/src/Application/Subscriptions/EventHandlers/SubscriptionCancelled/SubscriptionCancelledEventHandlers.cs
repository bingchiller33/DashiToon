using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Subscriptions.EventHandlers.SubscriptionCancelled;

public sealed class SubscriptionCancelledEventHandlers : INotificationHandler<SubscriptionCancelledEvent>
{
    private readonly IPaymentService _paymentService;

    public SubscriptionCancelledEventHandlers(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Handle(SubscriptionCancelledEvent notification, CancellationToken cancellationToken)
    {
        bool result = await _paymentService.CancelSubscription(
            notification.Subscription.SubscriptionId,
            notification.Reason);

        if (!result)
        {
            throw new Exception("Failed to cancel subscription");
        }
    }
}
