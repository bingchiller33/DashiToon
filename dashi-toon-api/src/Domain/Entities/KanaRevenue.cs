namespace DashiToon.Api.Domain.Entities;

public class KanaRevenue : BaseEntity<Guid>
{
    public string AuthorId { get; private set; } = null!;
    public decimal Revenue { get; private set; }
    public int FromChapterId { get; private set; }
    public DateTimeOffset Timestamp { get; set; }
    public string Reason { get; private set; } = null!;

    public Chapter FromChapter { get; private set; } = null!;

    private KanaRevenue()
    {
    }
}
