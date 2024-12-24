using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Moderation.EventHandlers.UserMuted;

public class UserMutedEventHandler : INotificationHandler<UserMutedEvent>
{
    protected readonly IApplicationDbContext _dbContext;

    public UserMutedEventHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UserMutedEvent mutedEvent, CancellationToken cancellationToken)
    {
        Notification notification = Notification.Create(
            mutedEvent.UserId,
            "Vi phạm tiêu chuẩn cộng đồng",
            $"Do vi phạm tiêu chuẩn cộng đồng. Bạn sẽ không thể bình luận hoặc đánh giá truyện cho đến hết ngày {mutedEvent.MutedUntil:d}"
        );

        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
