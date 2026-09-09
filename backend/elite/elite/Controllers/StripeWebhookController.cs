using elite.Interfaces;
using elite.Services;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;

namespace elite.Controllers
{
    [ApiController]
    [Route("api/webhooks/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;

        public StripeWebhookController(
            IConfiguration configuration,
            ILogger<StripeWebhookController> logger,
            IOrderService orderService,
            IPaymentService paymentService)
        {
            _configuration = configuration;
            _logger = logger;
            _orderService = orderService;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _configuration["Stripe:WebhookSecret"]
                );

                _logger.LogInformation("Stripe webhook event received: {EventType}", stripeEvent.Type);

                switch (stripeEvent.Type)
                {
                    case "checkout.session.completed":
                        var session = stripeEvent.Data.Object as Session;
                        await HandleCheckoutSessionCompleted(session);
                        break;

                    case "payment_intent.succeeded":
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentIntentSucceeded(paymentIntent);
                        break;

                    case "payment_intent.payment_failed":
                        var failedPaymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandlePaymentIntentFailed(failedPaymentIntent);
                        break;

                    case "checkout.session.expired":
                        var expiredSession = stripeEvent.Data.Object as Session;
                        await HandleCheckoutSessionExpired(expiredSession);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error: {Message}", ex.Message);
                return BadRequest();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook: {Message}", ex.Message);
                return StatusCode(500);
            }
        }

        private async Task HandleCheckoutSessionCompleted(Session session)
        {
            _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);

            if (session != null && session.Metadata.TryGetValue("order_id", out var orderIdStr) &&
                int.TryParse(orderIdStr, out var orderId))
            {
                // Update order status to completed
                await _orderService.UpdateOrderStatusAsync(orderId, "Completed");
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent paymentIntent)
        {
            _logger.LogInformation("Payment intent succeeded: {PaymentIntentId}", paymentIntent?.Id);

            if (paymentIntent != null && paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) &&
                int.TryParse(orderIdStr, out var orderId))
            {
                await _orderService.UpdateOrderStatusAsync(orderId, "Completed");
            }
        }

        private async Task HandlePaymentIntentFailed(PaymentIntent paymentIntent)
        {
            _logger.LogWarning("Payment intent failed: {PaymentIntentId}", paymentIntent?.Id);

            if (paymentIntent != null && paymentIntent.Metadata.TryGetValue("order_id", out var orderIdStr) &&
                int.TryParse(orderIdStr, out var orderId))
            {
                await _orderService.UpdateOrderStatusAsync(orderId, "Failed");
            }
        }

        private async Task HandleCheckoutSessionExpired(Session session)
        {
            _logger.LogInformation("Checkout session expired: {SessionId}", session?.Id);

            if (session != null && session.Metadata.TryGetValue("order_id", out var orderIdStr) &&
                int.TryParse(orderIdStr, out var orderId))
            {
                await _orderService.UpdateOrderStatusAsync(orderId, "Cancelled");
            }
        }
    }
}