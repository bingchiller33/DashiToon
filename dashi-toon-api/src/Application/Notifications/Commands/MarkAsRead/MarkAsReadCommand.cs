using DashiToon.Api.Application.Common.Interfaces;
using DashiToon.Api.Application.Common.Security;
using DashiToon.Api.Domain.Entities;

namespace DashiToon.Api.Application.Notifications.Commands.MarkAsRead;

[Authorize]
public sealed record MarkAsReadCommand(Guid NotificationId) : IRequest;

public sealed class MarkAsReadCommandHandler : IRequestHandler<MarkAsReadCommand>
{
    private readonly IApplicationDbContext _context;

    public MarkAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        Notification? notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken);

        if (notification is null)
        {
            throw new NotFoundException(request.NotificationId.ToString(), nameof(Notification));
        }

        notification.MarkAsRead();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
