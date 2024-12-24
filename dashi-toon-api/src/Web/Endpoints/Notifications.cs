using DashiToon.Api.Application.Common.Models;
using DashiToon.Api.Application.Notifications.Commands.MarkAllAsRead;
using DashiToon.Api.Application.Notifications.Commands.MarkAsRead;
using DashiToon.Api.Application.Notifications.Queries.GetUserNotifications;

namespace DashiToon.Api.Web.Endpoints;

public class Notifications : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        app.MapGroup(this)
            .RequireAuthorization()
            .MapGet(GetUserNotifications)
            .MapPut(MarkAsRead, "{notificationId}/mark-as-read")
            .MapPut(MarkAllAsRead, "mark-all-as-read");
    }

    public async Task<PaginatedList<NotificationVm>> GetUserNotifications(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await sender.Send(new GetUserNotificationsQuery(pageNumber, pageSize));
    }

    public async Task<IResult> MarkAsRead(ISender sender, Guid notificationId)
    {
        await sender.Send(new MarkAsReadCommand(notificationId));

        return Results.NoContent();
    }

    public async Task<IResult> MarkAllAsRead(ISender sender)
    {
        await sender.Send(new MarkAllAsReadCommand());

        return Results.NoContent();
    }
}
