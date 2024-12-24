using Microsoft.AspNetCore.SignalR;

namespace DashiToon.Api.Web.Hubs;

public class KonichiwassupHub : Hub<IKonichiwassupHub>
{
    public async Task Yahallo(string message)
    {
        await Clients.All.HelloConcac(message);
    }
}

public interface IKonichiwassupHub
{
    Task HelloConcac(string message);
}
