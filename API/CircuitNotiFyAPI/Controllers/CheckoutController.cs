using CircuitNotiFyAPI.Helpers;
using CircuitNotiFyAPI.Model;
using CircuitNotiFyAPI.Services;
using CircuitNotiFyAPI.Services.Intrerfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CircuitNotiFyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly IEnumerable<IPaymentService> _paymentServices;

        public CheckoutController(IEnumerable<IPaymentService> paymentServices)
        {
            _paymentServices = paymentServices;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessCheckout([FromBody] PaymentRequest request)
        {
            IPaymentService selectedService;
            string serviceName;

            switch (request.PaymentType)
            {
                case 1:
                    selectedService = _paymentServices.OfType<PayPalPaymentService>().FirstOrDefault();
                    serviceName = "PayPalPaymentService";
                    break;
                case 2:
                    selectedService = _paymentServices.OfType<StripePaymentService>().FirstOrDefault();
                    serviceName = "StripePaymentService";
                    break;
                default:
                    return BadRequest("Invalid PaymentType.");
            }

            if (selectedService == null)
            {
                return StatusCode(503, $"{serviceName} is not available.");
            }

            try
            {
                //Custom loop to make the service go to break mode
                for (int i = 0; i <= 20; i++)
                {
                    try
                    {
                        bool success = await selectedService.ProcessPaymentAsync(request.Amount);
                    }
                    catch (Exception ex)
                    {

                    }
                }

                return StatusCode(503, "Payment processing failed. Please try again later.");
            }
            catch (Exception ex)
            {
                return StatusCode(503, $"{serviceName} is currently unavailable. Please try again later.");
            }
        }

    }
}
