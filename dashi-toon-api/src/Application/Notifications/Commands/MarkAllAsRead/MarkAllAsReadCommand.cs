using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Notifications.Commands.MarkAllAsRead;

[Authorize]
public sealed record MarkAllAsReadCommand : IRequest;

public sealed class MarkAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public MarkAsReadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        List<Notification>? notification = await _context.Notifications
            .Where(n => n.UserId == _user.Id && n.IsRead == false)
            .ToListAsync(cancellationToken);

        notification.ForEach(n => n.MarkAsRead());
        await _context.SaveChangesAsync(cancellationToken);
    }
}
