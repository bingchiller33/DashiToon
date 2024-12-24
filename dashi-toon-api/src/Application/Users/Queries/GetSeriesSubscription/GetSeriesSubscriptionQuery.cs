using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Users.Queries.GetSubscribedSeries;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;

namespace DashiToon.Api.Application.Users.Queries.GetSeriesSubscription;

[Authorize]
public sealed record GetSeriesSubscriptionQuery(int SeriesId) : IRequest<SeriesSubscriptionVm?>;

public sealed class GetSeriesSubscriptionQueryHandler
    : IRequestHandler<GetSeriesSubscriptionQuery, SeriesSubscriptionVm?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;
    private readonly ICurrencyService _currencyService;

    public GetSeriesSubscriptionQueryHandler(
        IApplicationDbContext dbContext,
        IUser user,
        ICurrencyService currencyService)
    {
        _dbContext = dbContext;
        _user = user;
        _currencyService = currencyService;
    }

    public async Task<SeriesSubscriptionVm?> Handle(
        GetSeriesSubscriptionQuery request,
        CancellationToken cancellationToken)
    {
        Subscription? subscription = await _dbContext.Subscriptions
            .Include(s => s.Tier)
            .FirstOrDefaultAsync(
                s =>
                    s.UserId == _user.Id
                    && (s.Status == SubscriptionStatus.Active
                        || (s.Status == SubscriptionStatus.Cancelled
                            && s.NextBillingDate > DateTimeOffset.UtcNow
                        )
                    )
                    && s.Tier.SeriesId == request.SeriesId,
                cancellationToken);

        return subscription is null
            ? null
            : new SeriesSubscriptionVm(
                subscription.Id,
                new SubscriptionDashiFan(
                    subscription.Tier.Id,
                    subscription.Tier.Name,
                    subscription.Tier.Description,
                    subscription.Tier.Perks,
                    _currencyService.ConvertPrice(subscription.Tier.Price, "VND")
                ),
                subscription.Status,
                subscription.NextBillingDate.HasValue ? subscription.NextBillingDate.Value.ToString("O") : string.Empty
            );
    }
}
