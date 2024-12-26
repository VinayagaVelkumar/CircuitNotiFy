using CircuitNotiFyAPI.Model;

namespace CircuitNotiFyAPI.Helpers.Interfaces
{
    public interface INotificationHub
    {
        Task SendServiceState(ServiceMessage message);
    }
}
