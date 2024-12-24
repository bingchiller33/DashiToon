namespace DashiToon.Api.Domain.Services;

public class RevenueService
{
    public void ReceiveKanaRevenue(
        IDomainUser author,
        Chapter chapter,
        CommissionRate kanaCommissionRate,
        KanaExchangeRate exchangeRate)
    {
        if (kanaCommissionRate.Type != CommissionType.Kana)
        {
            throw new ArgumentException("Commission rate must be of type Kana.");
        }

        int kanaAmount = (int)Math.Floor((chapter.KanaPrice ?? 0) * ((100 - kanaCommissionRate.RatePercentage) / 100));

        decimal revenue = Math.Round(kanaAmount * exchangeRate.Rate, 2, MidpointRounding.ToNegativeInfinity);

        author.AddRevenueTransaction(RevenueTransaction.Create(
            RevenueType.Kana,
            RevenueTransactionType.Earn,
            revenue,
            $"Doanh thu từ chương [{chapter.ChapterNumber}] tập [{chapter.Volume.VolumeNumber}] truyện [{chapter.Volume.Series.Title}]",
            chapter.Volume.SeriesId
        ));
    }

    public void ReceiveDashiFanRevenue(
        IDomainUser author,
        Subscription subscription,
        SubscriptionOrder subscriptionOrder,
        CommissionRate dashiFanCommissionRate
    )
    {
        if (dashiFanCommissionRate.Type != CommissionType.DashiFan)
        {
            throw new ArgumentException("Commission rate must be of type DashiFan.");
        }

        decimal revenue = Math.Round(
            subscriptionOrder.Price.Amount * ((100 - dashiFanCommissionRate.RatePercentage) / 100),
            2,
            MidpointRounding.ToNegativeInfinity);

        author.AddRevenueTransaction(RevenueTransaction.Create(
            RevenueType.DashiFan,
            RevenueTransactionType.Earn,
            revenue,
            $"Doanh thu từ DashiFan [{subscription.Tier.Name}]",
            subscription.Tier.SeriesId
        ));
    }

    public void WithdrawRevenue(IDomainUser author, decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.");
        }

        if (author.Revenue < amount)
        {
            throw new OverWithdrawException();
        }

        author.AddRevenueTransaction(RevenueTransaction.Create(
            RevenueType.Withdrawal,
            RevenueTransactionType.Withdraw,
            amount * -1,
            "Rút tiền"
        ));
    }
}
