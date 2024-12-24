using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Subscriptions.EventHandlers.SubscriptionTierUpdated;

public sealed class SubscriptionTierUpdatedEventHandlers : INotificationHandler<SubscriptionTierUpdatedEvent>
{
    private readonly IPaymentService _paymentService;

    public SubscriptionTierUpdatedEventHandlers(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Handle(SubscriptionTierUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _paymentService.ReviseSubscriptionPlan(notification.Subscription);
    }
}
