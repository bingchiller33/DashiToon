using DashiToon.Api.Application.AuthorStudio.DashiFans.EventHandlers;
using DashiToon.Api.Application.Subscriptions.Commands.Models;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.ValueObjects;

namespace DashiToon.Api.Application.Common.Interfaces;

public interface IPaymentService
{
    Task<OrderResult> CreateOrder(Price price);
    Task<OrderResult> CaptureOrder(string orderId);
    Task<PlanResult> CreatePlan(DashiFan tier);
    Task<PlanResult> UpdatePlan(DashiFan tier);
    Task UpdatePlanPricing(DashiFan tier);
    Task UpdatePlanStatus(DashiFan tier);

    Task<SubscriptionResult> CreateSubscription(
        DashiFan tier,
        IDomainUser user,
        string returnUrl,
        string cancelUrl
    );

    Task<SubscriptionResult> GetSubscriptionDetail(Subscription subscription);
    Task<SubscriptionResult> ReviseSubscriptionPlan(Subscription notificationSubscription);
    Task<bool> ReactivateSubscription(string subscriptionId, string reason);
    Task<bool> SuspendSubscription(string subscriptionId, string reason);
    Task<bool> CancelSubscription(string subscriptionId, string reason);
    Task PayoutRevenue(string accountId, Price revenue);
}
