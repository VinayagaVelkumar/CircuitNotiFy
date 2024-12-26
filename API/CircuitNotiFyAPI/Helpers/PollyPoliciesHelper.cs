using CircuitNotiFyAPI.Helpers.Interfaces;
using CircuitNotiFyAPI.Model;
using Microsoft.AspNetCore.SignalR;
using Polly;

namespace CircuitNotiFyAPI.Helpers
{
    public class PollyPoliciesHelper
    {

        public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(string serviceName, IHubContext<NotificationHub, INotificationHub> hubContext = null)
        {
            return Policy<HttpResponseMessage>
             .Handle<HttpRequestException>()
             .Or<TimeoutException>()
             .OrResult(r => !r.IsSuccessStatusCode)
             .AdvancedCircuitBreakerAsync(
                 failureThreshold: 0.5,
                 samplingDuration: TimeSpan.FromSeconds(10),
                 minimumThroughput: 2,
                 durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: async (exception, timespan) =>
                    {
                        Console.WriteLine("Break");
                        ServiceMessage message = new ServiceMessage
                        {
                            ServiceName = serviceName,
                            State = "onBreak",
                            Message = "Service is in break mode"
                        };
                        await hubContext.Clients.All.SendServiceState(message);
                    },
                    onReset: async () =>
                    {
                        Console.WriteLine("Reset");
                        ServiceMessage message = new ServiceMessage
                        {
                            ServiceName = serviceName,
                            State = "open",
                            Message = "Service has recovered."
                        };
                        await hubContext.Clients.All.SendServiceState(message);
                    },
                    onHalfOpen: async () =>
                    {
                        Console.WriteLine("open");
                        ServiceMessage message = new ServiceMessage
                        {
                            ServiceName = serviceName,
                            State = "halfOpen",
                            Message = "Service is being tested for recovery."
                        };
                        await hubContext.Clients.All.SendServiceState(message);
                    });
        }

    }
}
