using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Users.Commands.UnsubscribeSeries;

[Authorize]
public sealed record UnsubscribeSeriesCommand(Guid SubscriptionId) : IRequest;

public sealed class UnsubscribeSeriesCommandHandler : IRequestHandler<UnsubscribeSeriesCommand>
{
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public UnsubscribeSeriesCommandHandler(IUser user, IUserRepository userRepository)
    {
        _user = user;
        _userRepository = userRepository;
    }

    public async Task Handle(UnsubscribeSeriesCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        Subscription? subscription = user.Subscriptions.FirstOrDefault(s =>
            s.Id == request.SubscriptionId && s.Status == SubscriptionStatus.Active);

        if (subscription is null)
        {
            throw new NotFoundException(request.SubscriptionId.ToString(), nameof(Subscription));
        }

        SubscriptionService.UnsubscribeSeries(subscription);

        await _userRepository.Update(user);
    }
}
