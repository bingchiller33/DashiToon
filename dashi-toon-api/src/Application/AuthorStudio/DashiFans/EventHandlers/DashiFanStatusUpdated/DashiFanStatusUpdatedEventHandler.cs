using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.EventHandlers.DashiFanStatusUpdated;

public sealed class DashiFanStatusUpdatedEventHandler : INotificationHandler<DashiFanStatusUpdatedEvent>
{
    private readonly IPaymentService _paymentService;

    public DashiFanStatusUpdatedEventHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Handle(DashiFanStatusUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _paymentService.UpdatePlanStatus(notification.Tier);
    }
}
