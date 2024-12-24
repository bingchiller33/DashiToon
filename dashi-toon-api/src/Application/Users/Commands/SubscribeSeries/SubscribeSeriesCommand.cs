using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Enums;
using DashiToon.Api.Domain.Services;

namespace DashiToon.Api.Application.Users.Commands.SubscribeSeries;

[Authorize]
public sealed record SubscribeSeriesCommand(int SeriesId, Guid TierId, string ReturnUrl, string CancelUrl)
    : IRequest<string>;

public sealed class SubscribeSeriesCommandHandler : IRequestHandler<SubscribeSeriesCommand, string>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPaymentService _paymentService;
    private readonly IUser _user;
    private readonly IUserRepository _userRepository;

    public SubscribeSeriesCommandHandler(
        IUser user,
        IUserRepository userRepository,
        IApplicationDbContext dbContext,
        IPaymentService paymentService)
    {
        _user = user;
        _userRepository = userRepository;
        _dbContext = dbContext;
        _paymentService = paymentService;
    }

    public async Task<string> Handle(SubscribeSeriesCommand request, CancellationToken cancellationToken)
    {
        IDomainUser? user = await _userRepository.GetById(_user.Id!);

        if (user is null)
        {
            throw new UnauthorizedAccessException();
        }

        Series? series = _dbContext.Series
            .Include(s => s.Tiers)
            .Where(s => s.Id == request.SeriesId
                        && s.Status != SeriesStatus.Trashed
                        && s.Status != SeriesStatus.Hiatus
                        && s.Status != SeriesStatus.Draft)
            .FirstOrDefault(s => s.Id == request.SeriesId);

        if (series is null)
        {
            throw new NotFoundException(request.SeriesId.ToString(), nameof(Series));
        }

        DashiFan? tier = series.Tiers.FirstOrDefault(df => df.Id == request.TierId && df.IsActive);

        if (tier is null)
        {
            throw new NotFoundException(request.TierId.ToString(), nameof(DashiFan));
        }

        Subscription subscription = SubscriptionService.SubscribeSeries(series, tier, user);

        SubscriptionResult result = await _paymentService.CreateSubscription(
            series.Tiers[0],
            user,
            request.ReturnUrl,
            request.CancelUrl);

        if (result.StatusCode != 201)
        {
            throw new Exception("Failed to create subscription");
        }

        subscription.CompleteCreation(result.Data.Id);

        _dbContext.Subscriptions.Add(subscription);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return result.Data.GetApprovalLink().Href;
    }
}
