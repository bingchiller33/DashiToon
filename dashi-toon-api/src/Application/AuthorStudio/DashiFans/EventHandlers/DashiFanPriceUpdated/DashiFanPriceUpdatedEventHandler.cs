using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.EventHandlers.DashiFanPriceUpdated;

public sealed class DashiFanPriceUpdatedEventHandler : INotificationHandler<DashiFanPriceUpdatedEvent>
{
    private readonly IPaymentService _paymentService;

    public DashiFanPriceUpdatedEventHandler(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task Handle(DashiFanPriceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _paymentService.UpdatePlanPricing(notification.Tier);
    }
}
