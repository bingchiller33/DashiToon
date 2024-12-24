using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.AuthorStudio.DashiFans.EventHandlers.DashiFanCreated;

public sealed class DashiFanCreatedEventHandler : INotificationHandler<DashiFanCreatedEvent>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPaymentService _paymentService;

    public DashiFanCreatedEventHandler(IPaymentService paymentService, IApplicationDbContext dbContext)
    {
        _paymentService = paymentService;
        _dbContext = dbContext;
    }

    public async Task Handle(DashiFanCreatedEvent notification, CancellationToken cancellationToken)
    {
        Domain.Entities.Series? series = await _dbContext.Series
            .FirstOrDefaultAsync(s => s.Id == notification.Tier.SeriesId, cancellationToken);

        PlanResult result = await _paymentService.CreatePlan(notification.Tier);

        notification.Tier.UpdatePlan(result.Data.Id);
    }
}
