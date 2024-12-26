using CircuitNotiFyAPI.Helpers;
using CircuitNotiFyAPI.Helpers.Interfaces;
using CircuitNotiFyAPI.Services.Intrerfaces;
using Microsoft.AspNetCore.SignalR;
using Polly;
using Polly.CircuitBreaker;

namespace CircuitNotiFyAPI.Services
{
    public class PayPalPaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public PayPalPaymentService(HttpClient httpClient, [FromKeyedServices("PayPalPolicy")] IAsyncPolicy<HttpResponseMessage> circuitBreakerPolicy)
        {
            _httpClient = httpClient;
            _circuitBreakerPolicy =circuitBreakerPolicy;
        }

        public async Task<bool> ProcessPaymentAsync(decimal amount)
        {

            try
            {
                var response = await _circuitBreakerPolicy.ExecuteAsync(async () =>
                {
                    return await _httpClient.GetAsync("500");
                });
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request successful!");
                    return true;
                }
                else
                {
                    throw new HttpRequestException($"Request failed with status code: {(int)response.StatusCode}");
                }
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine("Circuit is currently open. Request skipped.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}