namespace DashiToon.Api.Domain.Entities;

public class Follow : BaseEntity<Guid>
{
    private Follow()
    {
    }

    public int SeriesId { get; private set; }
    public string UserId { get; private set; } = null!;
    public bool IsNotified { get; private set; }
    public int? LatestChapterId { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }
    public Series Series { get; private set; } = null!;

    public static Follow CreateNew(
        int seriesId,
        string userId)
    {
        return new Follow
        {
            Id = Guid.NewGuid(),
            SeriesId = seriesId,
            UserId = userId,
            IsNotified = true,
            Timestamp = DateTimeOffset.UtcNow
        };
    }

    public void ChangeNotificationStatus(bool isNotified)
    {
        IsNotified = false;
    }

    public void LatestRead(int chapterId)
    {
        LatestChapterId = chapterId;
    }
}
