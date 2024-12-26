using CircuitNotiFyAPI.Helpers.Interfaces;
using CircuitNotiFyAPI.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CircuitNotiFyAPI.Helpers
{
    public class NotificationHub : Hub<INotificationHub>
    {
        public async Task SendServiceState(ServiceMessage message)
        {
            await Clients.All.SendServiceState(message);
        }

    }
}
