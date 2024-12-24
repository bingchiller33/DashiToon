using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Subscriptions.EventHandlers.SubscriptionSuspended;

public sealed class SubscriptionSuspendedEventHandlers : INotificationHandler<SubscriptionSuspendedEvent>
{
    private readonly IPaymentService _paymentService;

    public SubscriptionSuspendedEventHandlers(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Handle(SubscriptionSuspendedEvent notification, CancellationToken cancellationToken)
    {
        bool result = await _paymentService.SuspendSubscription(
            notification.Subscription.SubscriptionId,
            notification.Reason);

        if (!result)
        {
            throw new Exception("Failed to suspend subscription");
        }
    }
}
