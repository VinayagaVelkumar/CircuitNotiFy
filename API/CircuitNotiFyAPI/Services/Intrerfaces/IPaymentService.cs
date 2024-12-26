namespace CircuitNotiFyAPI.Services.Intrerfaces
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(decimal amount);
    }
}
