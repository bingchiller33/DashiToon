namespace DashiToon.Api.Domain.Entities;

public class Notification : BaseEntity<Guid>
{
    public string Title { get; private set; } = null!;
    public string Content { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTimeOffset Timestamp { get; private set; }

    public string UserId { get; private set; } = null!;

    public int? ChapterId { get; private set; }
    public Chapter? Chapter { get; private set; }

    private Notification()
    {
    }

    public static Notification Create(string recipientUserId, string title, string content, int? chapterId = null)
    {
        return new Notification
        {
            UserId = recipientUserId,
            Title = title,
            Content = content,
            IsRead = false,
            Timestamp = DateTimeOffset.UtcNow,
            ChapterId = chapterId
        };
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
