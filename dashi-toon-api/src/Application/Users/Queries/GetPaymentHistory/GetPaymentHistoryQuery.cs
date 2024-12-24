using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.Users.Queries.GetPaymentHistory;

[Authorize]
public sealed record GetPaymentHistoryQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<PaymentVm>>;

public sealed class GetPaymentHistoryQueryHandler : IRequestHandler<GetPaymentHistoryQuery, PaginatedList<PaymentVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;
    private readonly ICurrencyService _currencyService;

    public GetPaymentHistoryQueryHandler(IUser user, IApplicationDbContext dbContext, ICurrencyService currencyService)
    {
        _user = user;
        _dbContext = dbContext;
        _currencyService = currencyService;
    }

    public async Task<PaginatedList<PaymentVm>> Handle(GetPaymentHistoryQuery request,
        CancellationToken cancellationToken)
    {
        List<PaymentVm> subscriptionOrder = await _dbContext.SubscriptionOrders
            .AsNoTracking()
            .Include(so => so.Subscription)
            .ThenInclude(so => so.Tier)
            .ThenInclude(so => so.Series)
            .Where(so => so.UserId == _user.Id)
            .Select(so => new PaymentVm(
                so.Id,
                so.UpgradeTierId.HasValue
                    ? $"Thăng hạng DashiFan {so.Subscription.Tier.Name} cho truyện {so.Subscription.Tier.Series.Title}"
                    : $"Trở thành DashiFan {so.Subscription.Tier.Name} cho truyện {so.Subscription.Tier.Series.Title}",
                so.Price,
                so.Status,
                so.CompletedAt.HasValue ? so.CompletedAt.Value.ToString("O") : ""
            )).ToListAsync(cancellationToken);

        List<PaymentVm> purchaseOrder = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.KanaGoldPack)
            .Where(po => po.UserId == _user.Id)
            .Select(po => new PaymentVm(
                po.Id,
                $"Mua gói {po.KanaGoldPack.Amount} Kana Gold",
                po.Price,
                po.Status,
                po.CompletedAt.HasValue ? po.CompletedAt.Value.ToString("O") : ""
            )).ToListAsync(cancellationToken);

        List<PaymentVm> result = subscriptionOrder.Concat(purchaseOrder).ToList();

        result.ForEach(x => x.Price = x.Price is null ? null : _currencyService.ConvertPrice(x.Price, "VND"));

        return new PaginatedList<PaymentVm>(
            result
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList(),
            result.Count, request.PageNumber, request.PageSize);
    }
}
