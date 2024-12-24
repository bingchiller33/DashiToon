using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Domain.Entities;
using DashiToon.Api.Domain.Events;

namespace DashiToon.Api.Application.Moderation.EventHandlers.AuthorRestricted;

public class AuthorRestrictedEventHandler : INotificationHandler<AuthorRestrictedEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public AuthorRestrictedEventHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(AuthorRestrictedEvent restrictedEvent, CancellationToken cancellationToken)
    {
        Notification? notification = Notification.Create(
            restrictedEvent.UserId,
            "Vi phạm chính sách về xuất bản",
            $"Do vi phạm chính sách về xuất bản. Bạn đã bị giới hạn tất cả các quyền sửa đổi truyện cho tới ngày {restrictedEvent.RestrictedUntil:d}"
        );

        _dbContext.Notifications.Add(notification);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
