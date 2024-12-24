using DashiToon.Api.Application.Common.Exceptions;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Users.Commands.DowngradeSubscriptionTier;

[Authorize]
public sealed record DowngradeSubscriptionTierCommand(Guid SubscriptionId, Guid TierId) : IRequest;

public sealed class DowngradeSubscriptionTierCommandHandler : IRequestHandler<DowngradeSubscriptionTierCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public DowngradeSubscriptionTierCommandHandler(
        IApplicationDbContext dbContext,
        IUserRepository userRepository,
        IUser user)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _user = user;
    }

    public async Task Handle(DowngradeSubscriptionTierCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id ?? string.Empty);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        Subscription? subscription = user.Subscriptions.FirstOrDefault(s => s.Id == request.SubscriptionId);

        if (subscription is null)
        {
            throw new ForbiddenAccessException();
        }

        DashiFan? tier = _dbContext.DashiFans
            .FirstOrDefault(df => df.Id == request.TierId
                                  && df.SeriesId == subscription.Tier.SeriesId
                                  && df.IsActive);

        if (tier is null)
        {
            throw new NotFoundException(request.TierId.ToString(), nameof(DashiFan));
        }

        SubscriptionService.DowngradeTier(subscription, tier);

        await _userRepository.Update(user);
    }
}
