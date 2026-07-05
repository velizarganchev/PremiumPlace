using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PremiumPlace.DTO.PayPal;
using PremiumPlace_API.Infrastructure.Payments.PayPal;

namespace PremiumPlace_API.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly PayPalOptions _paypal;

        public PaymentsController(IOptions<PayPalOptions> paypal)
        {
            _paypal = paypal.Value;
        }

        // Public: the SPA needs the (non-secret) client id + currency to load the PayPal SDK.
        [HttpGet("config")]
        public IActionResult GetConfig() => Ok(new PaymentConfigDTO
        {
            ClientId = _paypal.ClientId,
            Currency = string.IsNullOrWhiteSpace(_paypal.ExpectedCurrency) ? "EUR" : _paypal.ExpectedCurrency!
        });
    }
}
