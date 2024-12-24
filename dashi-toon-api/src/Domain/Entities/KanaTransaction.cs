namespace DashiToon.Api.Domain.Entities;

public class KanaTransaction : BaseEntity<Guid>
{
    private KanaTransaction()
    {
    }

    public string UserId { get; private set; } = null!;
    public KanaType Currency { get; private set; }
    public TransactionType Type { get; private set; }
    public int Amount { get; private set; }
    public string Reason { get; private set; } = null!;
    public DateTimeOffset Timestamp { get; private set; }
    public int? ChapterId { get; private set; }
    public Chapter? Chapter { get; private set; }

    public static KanaTransaction Create(
        KanaType currency,
        TransactionType type,
        int amount,
        string reason,
        int? chapterId = null)
    {
        return new KanaTransaction
        {
            Id = Guid.NewGuid(),
            Currency = currency,
            Type = type,
            Amount = amount,
            Reason = reason,
            Timestamp = DateTimeOffset.UtcNow,
            ChapterId = chapterId
        };
    }
}
