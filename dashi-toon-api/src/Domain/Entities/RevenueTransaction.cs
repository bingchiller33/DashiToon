namespace DashiToon.Api.Domain.Entities;

public class RevenueTransaction : BaseAuditableEntity<Guid>
{
    public string AuthorId { get; private set; } = null!;
    public RevenueType Type { get; private set; }
    public RevenueTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public string Reason { get; private set; } = null!;
    public int? SeriesId { get; private set; }
    public Series? Series { get; private set; }

    private RevenueTransaction()
    {
    }

    public static RevenueTransaction Create(
        RevenueType revenueType,
        RevenueTransactionType transactionType,
        decimal amount,
        string reason,
        int? fromSeries = null)
    {
        return new RevenueTransaction
        {
            Id = Guid.NewGuid(),
            Type = revenueType,
            TransactionType = transactionType,
            Amount = amount,
            Timestamp = DateTimeOffset.UtcNow,
            Reason = reason,
            SeriesId = fromSeries
        };
    }
}
