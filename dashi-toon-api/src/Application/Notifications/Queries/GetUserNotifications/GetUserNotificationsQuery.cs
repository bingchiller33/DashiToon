using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Mappings;
using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Common.Security;

namespace DashiToon.Api.Application.Notifications.Queries.GetUserNotifications;

[Authorize]
public sealed record GetUserNotificationsQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PaginatedList<NotificationVm>>;

public sealed class GetUserNotificationsQueryHandler
    : IRequestHandler<GetUserNotificationsQuery, PaginatedList<NotificationVm>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IUser _user;

    public GetUserNotificationsQueryHandler(IApplicationDbContext dbContext, IUser user)
    {
        _dbContext = dbContext;
        _user = user;
    }

    public async Task<PaginatedList<NotificationVm>> Handle(GetUserNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Notifications
            .Include(n => n.Chapter)
            .ThenInclude(c => c!.Volume)
            .ThenInclude(s => s.Series)
            .Where(n => n.UserId == _user.Id && n.Timestamp > DateTimeOffset.UtcNow - TimeSpan.FromDays(30))
            .OrderBy(n => n.IsRead)
            .Select(n => new NotificationVm(
                n.Id,
                n.Title,
                n.Content,
                n.IsRead,
                n.Timestamp.ToString("O"),
                n.ChapterId,
                n.ChapterId.HasValue ? n.Chapter!.VolumeId : null,
                n.ChapterId.HasValue ? n.Chapter!.Volume.SeriesId : null,
                n.ChapterId.HasValue ? n.Chapter!.Volume.Series.Type : null
            ))
            .PaginatedListAsync(request.PageNumber, request.PageSize);
    }
}
