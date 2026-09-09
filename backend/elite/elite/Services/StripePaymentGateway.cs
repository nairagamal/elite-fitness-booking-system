using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Stripe;

using Stripe.Checkout;

namespace elite.Services
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripePaymentGateway> _logger;

        public string GatewayName => "Stripe";

        public StripePaymentGateway(IConfiguration configuration, ILogger<StripePaymentGateway> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public bool SupportsPaymentMethod(string paymentMethod)
        {
            var supportedMethods = new[] {
                "stripe", "card", "creditcard", "mada",
                "applepay", "googlepay", "klarna", "afterpay"
            };
            return supportedMethods.Contains(paymentMethod?.ToLower());
        }

        public async Task<CheckoutResponse> CreateCheckoutAsync(CheckoutRequest request)
        {
            try
            {
                _logger.LogInformation("Creating Stripe checkout session for order {OrderId}", request.OrderId);

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = GetPaymentMethodTypes(request.PaymentMethod),
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Currency.ToLower(),
                        UnitAmount = (long)(request.Amount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"Order #{request.OrderId}",
                            Description = $"Payment for order {request.OrderId}",
                        },
                    },
                    Quantity = 1,
                },
            },
                    Mode = "payment",
                    SuccessUrl = request.ReturnUrl ?? $"{GetBaseUrl()}/payment/success?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = request.CancelUrl ?? $"{GetBaseUrl()}/payment/cancel",
                    ClientReferenceId = request.OrderId.ToString(),
                    Metadata = new Dictionary<string, string>
            {
                { "order_id", request.OrderId.ToString() },
                { "customer_email", request.CustomerEmail ?? "" },
                { "customer_phone", request.CustomerPhone ?? "" }
            },
                };

                if (!string.IsNullOrEmpty(request.CustomerEmail))
                {
                    options.CustomerEmail = request.CustomerEmail;
                }

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                // Fix: ExpiresAt is a DateTime directly, not nullable
                // Check if it's the default value (MinValue) to determine if it's set
                DateTime expiresAt;
                if (session.ExpiresAt != DateTime.MinValue)
                {
                    expiresAt = session.ExpiresAt;
                }
                else
                {
                    expiresAt = DateTime.UtcNow.AddHours(24);
                }

                return new CheckoutResponse
                {
                    Success = true,
                    Message = "Stripe checkout session created successfully",
                    CheckoutId = session.Id,
                    CheckoutUrl = session.Url,
                    ExpiresAt = expiresAt,
                    Metadata = new Dictionary<string, object>
            {
                { "gateway", GatewayName },
                { "session_id", session.Id },
                { "payment_intent", session.PaymentIntentId }
            }
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating checkout: {Message}", ex.Message);
                return new CheckoutResponse
                {
                    Success = false,
                    Message = $"Stripe error: {ex.Message}",
                    Metadata = new Dictionary<string, object>
            {
                { "error_type", ex.StripeError?.Type },
                { "error_code", ex.StripeError?.Code }
            }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe checkout session");
                throw;
            }
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(string checkoutId, PaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing Stripe payment for session {CheckoutId}", checkoutId);

                var sessionService = new SessionService();
                var session = await sessionService.GetAsync(checkoutId);

                if (session == null)
                {
                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Checkout session not found",
                        Status = "failed"
                    };
                }

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

                var isSuccess = paymentIntent.Status == "succeeded";

                return new PaymentResponse
                {
                    Success = isSuccess,
                    Message = isSuccess ? "Payment processed successfully" : $"Payment status: {paymentIntent.Status}",
                    PaymentId = paymentIntent.Id,
                    CheckoutId = checkoutId,
                    Status = MapStripeStatus(paymentIntent.Status),
                    Amount = (decimal)(paymentIntent.Amount / 100.0),
                    Currency = paymentIntent.Currency,
                    CreatedAt = paymentIntent.Created,
                    ProcessedAt = paymentIntent.Status == "succeeded" ? DateTime.UtcNow : (DateTime?)null,
                    Metadata = new Dictionary<string, object>
            {
                { "gateway", GatewayName },
                { "stripe_status", paymentIntent.Status },
                { "payment_method", paymentIntent.PaymentMethodTypes?.FirstOrDefault() }
            }
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error processing payment: {Message}", ex.Message);
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Stripe error: {ex.Message}",
                    Status = "failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe payment");
                throw;
            }
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("Verifying Stripe payment {PaymentId}", paymentId);

                // Try to get as PaymentIntent first
                var paymentIntentService = new PaymentIntentService();

                try
                {
                    var paymentIntent = await paymentIntentService.GetAsync(paymentId);

                    return new PaymentResponse
                    {
                        Success = paymentIntent.Status == "succeeded",
                        Message = $"Payment status: {paymentIntent.Status}",
                        PaymentId = paymentIntent.Id,
                        Status = MapStripeStatus(paymentIntent.Status),
                        Amount = (decimal)(paymentIntent.Amount / 100.0),
                        Currency = paymentIntent.Currency,
                        CreatedAt = paymentIntent.Created,
                        ProcessedAt = paymentIntent.Status == "succeeded" ? DateTime.UtcNow : null,
                        Metadata = new Dictionary<string, object>
                        {
                            { "gateway", GatewayName },
                            { "stripe_status", paymentIntent.Status }
                        }
                    };
                }
                catch (StripeException)
                {
                    // If not a PaymentIntent, try as Checkout Session
                    var sessionService = new SessionService();
                    var session = await sessionService.GetAsync(paymentId);

                    if (session?.PaymentIntentId != null)
                    {
                        var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

                        return new PaymentResponse
                        {
                            Success = paymentIntent.Status == "succeeded",
                            Message = $"Payment status: {paymentIntent.Status}",
                            PaymentId = paymentIntent.Id,
                            Status = MapStripeStatus(paymentIntent.Status),
                            Amount = (decimal)(paymentIntent.Amount / 100.0),
                            Currency = paymentIntent.Currency,
                            CreatedAt = paymentIntent.Created,
                            Metadata = new Dictionary<string, object>
                            {
                                { "gateway", GatewayName },
                                { "session_id", session.Id }
                            }
                        };
                    }

                    return new PaymentResponse
                    {
                        Success = false,
                        Message = "Payment not found",
                        Status = "failed"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Stripe payment");
                throw;
            }
        }

        public async Task<PaymentResponse> CancelPaymentAsync(string paymentId)
        {
            try
            {
                _logger.LogInformation("Cancelling Stripe payment {PaymentId}", paymentId);

                var paymentIntentService = new PaymentIntentService();
                var cancelledIntent = await paymentIntentService.CancelAsync(paymentId);

                return new PaymentResponse
                {
                    Success = cancelledIntent.Status == "canceled",
                    Message = "Payment cancelled successfully",
                    PaymentId = cancelledIntent.Id,
                    Status = "cancelled",
                    Amount = (decimal)(cancelledIntent.Amount / 100.0),
                    Currency = cancelledIntent.Currency,
                    ProcessedAt = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object>
                    {
                        { "gateway", GatewayName },
                        { "cancelled_at", DateTime.UtcNow.ToString("o") }
                    }
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error cancelling payment: {Message}", ex.Message);
                return new PaymentResponse
                {
                    Success = false,
                    Message = $"Stripe error: {ex.Message}",
                    Status = "failed"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling Stripe payment");
                throw;
            }
        }

        private List<string> GetPaymentMethodTypes(string paymentMethod)
        {
            return paymentMethod?.ToLower() switch
            {
                "card" or "creditcard" => new List<string> { "card" },
                "applepay" => new List<string> { "card", "apple_pay" },
                "googlepay" => new List<string> { "card", "google_pay" },
                "klarna" => new List<string> { "klarna" },
                "afterpay" => new List<string> { "afterpay_clearpay" },
                "mada" => new List<string> { "card" }, // Mada cards work with standard card payments
                _ => new List<string> { "card" }
            };
        }

        private string MapStripeStatus(string stripeStatus)
        {
            return stripeStatus switch
            {
                "succeeded" => "success",
                "requires_payment_method" => "pending",
                "requires_confirmation" => "pending",
                "requires_action" => "pending",
                "processing" => "processing",
                "requires_capture" => "pending",
                "canceled" => "cancelled",
                _ => "failed"
            };
        }

        private string GetBaseUrl()
        {
            // In production, get from configuration
            return _configuration["App:BaseUrl"] ?? "https://localhost:7003";
        }
    }
}