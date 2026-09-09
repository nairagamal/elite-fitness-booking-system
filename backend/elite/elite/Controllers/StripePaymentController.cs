using elite.Models;
using elite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StripePaymentController : ControllerBase
    {
        private readonly StripeDirectPaymentService _stripeService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentController> _logger;

        public StripePaymentController(
            StripeDirectPaymentService stripeService,
            IConfiguration configuration,
            ILogger<StripePaymentController> logger)
        {
            _stripeService = stripeService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("publishable-key")]
        [AllowAnonymous]
        public IActionResult GetPublishableKey()
        {
            return Ok(new { publishableKey = _configuration["Stripe:PublishableKey"] });
        }

        [HttpPost("create-setup-intent")]
        public async Task<IActionResult> CreateSetupIntent([FromBody] string customerId = null)
        {
            try
            {
                var result = await _stripeService.CreateSetupIntent(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating setup intent");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("create-customer")]
        public async Task<IActionResult> CreateCustomer([FromBody] StripeCustomerRequest request)
        {
            try
            {
                var customer = await _stripeService.CreateCustomer(request);
                return Ok(new { customerId = customer.Id });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating customer");
                return BadRequest(new { message = ex.StripeError?.Message, type = ex.StripeError?.Type });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] StripePaymentRequest request)
        {
            try
            {
                var paymentIntent = await _stripeService.CreatePaymentIntent(request);

                return Ok(new
                {
                    clientSecret = paymentIntent.ClientSecret,
                    paymentIntentId = paymentIntent.Id,
                    status = paymentIntent.Status,
                    requiresAction = paymentIntent.Status == "requires_action",
                    nextAction = paymentIntent.NextAction
                });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent");

                // Check if payment requires authentication
                if (ex.StripeError?.PaymentIntent != null)
                {
                    return Ok(new
                    {
                        requiresAction = true,
                        paymentIntentId = ex.StripeError.PaymentIntent.Id,
                        clientSecret = ex.StripeError.PaymentIntent.ClientSecret,
                        status = ex.StripeError.PaymentIntent.Status
                    });
                }

                return BadRequest(new { message = ex.StripeError?.Message, type = ex.StripeError?.Type });
            }
        }

        [HttpPost("confirm-payment-intent/{paymentIntentId}")]
        public async Task<IActionResult> ConfirmPaymentIntent(string paymentIntentId, [FromBody] string paymentMethodId = null)
        {
            try
            {
                var paymentIntent = await _stripeService.ConfirmPaymentIntent(paymentIntentId, paymentMethodId);

                return Ok(new
                {
                    status = paymentIntent.Status,
                    paymentIntentId = paymentIntent.Id,
                    requiresAction = paymentIntent.Status == "requires_action",
                    nextAction = paymentIntent.NextAction
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { message = ex.StripeError?.Message });
            }
        }

        [HttpGet("payment-intent/{paymentIntentId}")]
        public async Task<IActionResult> GetPaymentIntent(string paymentIntentId)
        {
            try
            {
                var paymentIntent = await _stripeService.GetPaymentIntent(paymentIntentId);

                return Ok(new
                {
                    status = paymentIntent.Status,
                    amount = paymentIntent.Amount / 100.0m,
                    currency = paymentIntent.Currency,
                    created = paymentIntent.Created,
                    paymentMethod = paymentIntent.PaymentMethodId
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}