// Controllers/PaymentController.cs
using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using elite.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        // Add this method to your existing PaymentController.cs

        [HttpPost("stripe/create-checkout")]
        public async Task<IActionResult> CreateStripeCheckout([FromBody] CheckoutRequest request)
        {
            try
            {
                request.PaymentMethod = "stripe";
                var result = await _paymentService.CreateCheckoutAsync(request);

                if (!result.Success)
                    return BadRequest(new { message = result.Message });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe checkout");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("stripe/process-webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessStripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            // The actual webhook handling is in StripeWebhookController
            return Ok();
        }

        [HttpPost("create-checkout")]
        public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request)
        {
            try
            {
                _logger.LogInformation("Creating checkout for order {OrderId}", request.OrderId);

                var result = await _paymentService.CreateCheckoutAsync(request);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating checkout for order {OrderId}", request.OrderId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromQuery] string checkoutId, [FromBody] PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing payment for checkout {CheckoutId}", checkoutId);

                if (string.IsNullOrEmpty(checkoutId))
                {
                    return BadRequest(new { message = "Checkout ID is required" });
                }

                var result = await _paymentService.ProcessPaymentAsync(checkoutId, request);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for checkout {CheckoutId}", checkoutId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("verify")]
        public async Task<IActionResult> VerifyPayment([FromQuery] string paymentId)
        {
            try
            {
                _logger.LogInformation("Verifying payment {PaymentId}", paymentId);

                if (string.IsNullOrEmpty(paymentId))
                {
                    return BadRequest(new { message = "Payment ID is required" });
                }

                var result = await _paymentService.VerifyPaymentAsync(paymentId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment {PaymentId}", paymentId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelPayment([FromQuery] string paymentId)
        {
            try
            {
                _logger.LogInformation("Cancelling payment {PaymentId}", paymentId);

                if (string.IsNullOrEmpty(paymentId))
                {
                    return BadRequest(new { message = "Payment ID is required" });
                }

                var result = await _paymentService.CancelPaymentAsync(paymentId);

                if (!result.Success)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling payment {PaymentId}", paymentId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("methods")]
        [AllowAnonymous]
        public IActionResult GetPaymentMethods()
        {
            var methods = new[]
            {
                new { Id = "mock", Name = "Mock Payment", Description = "Test payment method" },
                new { Id = "mada", Name = "Mada", Description = "Mada debit card" },
                new { Id = "creditcard", Name = "Credit Card", Description = "Visa, MasterCard, etc." },
                new { Id = "applepay", Name = "Apple Pay", Description = "Apple Pay mobile payment" },
                new { Id = "tabby", Name = "Tabby", Description = "Buy now, pay later" },
                new { Id = "tamara", Name = "Tamara", Description = "Shop now, pay later" }
            };

            return Ok(methods);
        }
    }
}