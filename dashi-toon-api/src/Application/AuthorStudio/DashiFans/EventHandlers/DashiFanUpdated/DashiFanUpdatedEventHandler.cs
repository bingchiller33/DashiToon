using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.EventHandlers.DashiFanUpdated;

public sealed class DashiFanUpdatedEventHandler : INotificationHandler<DashiFanUpdatedEvent>
{
    private readonly IPaymentService _paymentService;

    public DashiFanUpdatedEventHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Handle(DashiFanUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _paymentService.UpdatePlan(notification.Tier);
    }
}
