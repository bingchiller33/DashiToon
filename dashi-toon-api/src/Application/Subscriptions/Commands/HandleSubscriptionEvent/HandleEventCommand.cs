using System.Text.Json;
using System.Text.Json.Nodes;
using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Services;
using DashiToon.Api.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace DashiToon.Api.Application.Subscriptions.Commands.HandleSubscriptionEvent;

public sealed record HandleEventCommand(string Data) : IRequest;

public sealed class HandleEventCommandHandler : IRequestHandler<HandleEventCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUserRepository _userRepository;
    private readonly Dictionary<string, Func<JsonObject, Task>> _handlers;
    private readonly ILogger<HandleEventCommandHandler> _logger;
    private readonly ICurrencyService _currencyService;

    public HandleEventCommandHandler(
        IApplicationDbContext dbContext,
        IUserRepository userRepository,
        ILogger<HandleEventCommandHandler> logger,
        ICurrencyService currencyService)
    {
        _dbContext = dbContext;
        _userRepository = userRepository;
        _logger = logger;
        _currencyService = currencyService;
        _handlers = new Dictionary<string, Func<JsonObject, Task>>
        {
            { "PAYMENT.SALE.COMPLETED", SubscriptionPaymentCompletedHandler },
            { "BILLING.SUBSCRIPTION.ACTIVATED", SubscriptionActivatedHandler },
            { "BILLING.SUBSCRIPTION.EXPIRED", SubscriptionExpiredHandler },
            { "BILLING.SUBSCRIPTION.CANCELLED", SubscriptionCancelledHandler },
            { "BILLING.SUBSCRIPTION.SUSPENDED", SubscriptionSuspendedHandler },
            { "BILLING.SUBSCRIPTION.PAYMENT.FAILED", SubscriptionPaymentFailedHandler }
        };
    }

    public async Task Handle(HandleEventCommand request, CancellationToken cancellationToken)
    {
        EventInfo? data = JsonSerializer.Deserialize<EventInfo>(
            request.Data,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower });

        _logger.LogInformation("Event Data: {Data}", data);

        if (_handlers.TryGetValue(data!.EventType, out Func<JsonObject, Task>? handler))
        {
            await handler(data.Resource);
        }
    }

    private async Task SubscriptionPaymentCompletedHandler(JsonObject data)
    {
        SubscriptionPaymentCompleted? result = data.Deserialize<SubscriptionPaymentCompleted>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        if (result is null)
        {
            return;
        }

        Subscription? subscription = await _dbContext.Subscriptions
            .Include(s => s.Tier)
            .ThenInclude(t => t.Series)
            .FirstOrDefaultAsync(s => s.SubscriptionId == result.BillingAgreementId);

        if (subscription is not null)
        {
            Price? price = _currencyService.ConvertPrice(
                Price.CreateNew(
                    decimal.Parse(result.Amount.Total),
                    result.Amount.Currency),
                "VND");

            SubscriptionOrder subscriptionOrder = SubscriptionOrder.CreateNew(
                result.Id,
                subscription.UserId,
                subscription.Id,
                Price.CreateNew(
                    Math.Round(price.Amount, 2, MidpointRounding.ToPositiveInfinity),
                    price.Currency
                )
            );

            subscriptionOrder.CompleteOrder();

            IDomainUser? author = await _userRepository.GetById(subscription.Tier.Series.CreatedBy!);

            if (author is not null)
            {
                RevenueService? revenueService = new();

                CommissionRate? commissionRate = await _dbContext.CommissionRates
                    .FirstAsync(cr => cr.Type == CommissionType.DashiFan);

                revenueService.ReceiveDashiFanRevenue(author, subscription, subscriptionOrder, commissionRate);
            }

            _dbContext.SubscriptionOrders.Add(subscriptionOrder);

            await _dbContext.SaveChangesAsync(default);
        }
    }

    private async Task SubscriptionActivatedHandler(JsonObject data)
    {
        SubscriptionActivated? result = data.Deserialize<SubscriptionActivated>(new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        });

        if (result is null)
        {
            return;
        }

        Subscription? subscription = await _dbContext.Subscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == result.Id);

        if (subscription is not null)
        {
            subscription.Activate(result.BillingInfo.NextBillingTime);

            await _dbContext.SaveChangesAsync(default);
        }
    }

    private Task SubscriptionExpiredHandler(JsonObject data)
    {
        return Task.CompletedTask;
    }

    private Task SubscriptionCancelledHandler(JsonObject data)
    {
        return Task.CompletedTask;
    }

    private Task SubscriptionSuspendedHandler(JsonObject data)
    {
        return Task.CompletedTask;
    }

    private Task SubscriptionPaymentFailedHandler(JsonObject data)
    {
        return Task.CompletedTask;
    }
}
