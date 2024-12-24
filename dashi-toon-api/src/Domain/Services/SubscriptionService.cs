namespace DashiToon.Api.Domain.Services;

public class SubscriptionService
{
    public static Subscription SubscribeSeries(Series series, DashiFan tier, IDomainUser user)
    {
        if (IsUserSubscribedToSeries(user, series.Id))
        {
            throw new AlreadySubscribedException();
        }

        IEnumerable<Subscription> pendingSubscriptions = user.Subscriptions
            .Where(s => s.Tier.SeriesId == series.Id && s.Status is SubscriptionStatus.Pending);

        foreach (Subscription subscription in pendingSubscriptions)
        {
            subscription.Cancel();
        }

        return Subscription.Create(user.Id, tier.Id);
    }

    public static void UnsubscribeSeries(Subscription subscription)
    {
        subscription.Cancel();
    }

    public static bool IsUserSubscribedToSeries(IDomainUser user, int seriesId)
    {
        return user.Subscriptions
            .Where(s => s.Tier.SeriesId == seriesId)
            .Any(ss => ss.Status is SubscriptionStatus.Active
                       || (ss.Status is SubscriptionStatus.Cancelled && ss.NextBillingDate > DateTimeOffset.UtcNow));
    }

    public static Price UpgradeTier(Subscription subscription, DashiFan tier)
    {
        if (subscription.Status is not SubscriptionStatus.Active)
        {
            throw new ChangeSubscriptionTierException();
        }

        if (subscription.Tier.Price.Amount >= tier.Price.Amount)
        {
            throw new Exception("Invalid upgrade tier");
        }

        if (tier.Price.Currency != subscription.Tier.Price.Currency)
        {
            throw new Exception("Currency mismatch");
        }

        return Price.CreateNew(
            tier.Price.Amount - subscription.Tier.Price.Amount,
            subscription.Tier.Price.Currency);
    }

    public static void DowngradeTier(Subscription subscription, DashiFan tier)
    {
        if (subscription.Status is not SubscriptionStatus.Active)
        {
            throw new ChangeSubscriptionTierException();
        }

        subscription.ChangeTier(tier);
    }
}
