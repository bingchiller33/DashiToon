using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Users.Queries.GetSubscribedSeries;

[Authorize]
public sealed record GetSubscriptionsQuery : IRequest<List<UserSubscriptionVm>>;

public sealed class GetSubscriptionQueryHandler
    : IRequestHandler<GetSubscriptionsQuery, List<UserSubscriptionVm>>
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IImageStore _imageStore;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;
    private readonly ICurrencyService _currencyService;

    public GetSubscriptionQueryHandler(
        IUser user,
        IUserRepository userRepository,
        IApplicationDbContext context,
        IImageStore imageStore,
        IIdentityService identityService,
        ICurrencyService currencyService)
    {
        _user = user;
        _userRepository = userRepository;
        _context = context;
        _imageStore = imageStore;
        _identityService = identityService;
        _currencyService = currencyService;
    }

    public async Task<List<UserSubscriptionVm>> Handle(GetSubscriptionsQuery request,
        CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id!);

        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        IEnumerable<Guid> dashiFanIds = user.Subscriptions.Select(s => s.DashiFanId);

        Dictionary<Guid, DashiFan> dashiFans = await _context.DashiFans
            .Include(df => df.Series)
            .Where(df => dashiFanIds.Contains(df.Id))
            .ToDictionaryAsync(
                df => df.Id,
                df => df, cancellationToken);

        return user.Subscriptions
            .Select(s =>
            {
                DashiFan? dashiFan = dashiFans.GetValueOrDefault(s.DashiFanId);

                return new UserSubscriptionVm(
                    s.Id,
                    new SubscriptionSeries(
                        dashiFan?.SeriesId,
                        string.IsNullOrEmpty(dashiFan?.Series.Thumbnail)
                            ? _imageStore
                                .GetUrl("thumbnails/default.png", DateTime.UtcNow.AddMinutes(2))
                                .GetAwaiter()
                                .GetResult()
                            : _imageStore
                                .GetUrl($"thumbnails/{dashiFan.Series.Thumbnail}", DateTime.UtcNow.AddMinutes(2))
                                .GetAwaiter()
                                .GetResult(),
                        dashiFan?.Series.Title,
                        string.Join(",", dashiFan?.Series.Authors ?? []),
                        dashiFan?.LastModified.ToString("O")
                    ),
                    new SubscriptionDashiFan(
                        dashiFan?.Id,
                        dashiFan?.Name,
                        dashiFan?.Description,
                        dashiFan?.Perks,
                        dashiFan is null ? null : _currencyService.ConvertPrice(dashiFan.Price, "VND")
                    ),
                    s.NextBillingDate.HasValue ? s.NextBillingDate.Value.ToString("O") : "",
                    s.Created.ToString("O"),
                    s.Status
                );
            })
            .OrderByDescending(s => s.Status)
            .ToList();
    }
}
