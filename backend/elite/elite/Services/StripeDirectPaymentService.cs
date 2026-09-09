using elite.DTOs;
using elite.Interfaces;
using elite.Models;
using Stripe;

namespace elite.Services
{
    public class StripeDirectPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeDirectPaymentService> _logger;

        public StripeDirectPaymentService(IConfiguration configuration, ILogger<StripeDirectPaymentService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<StripeSetupIntentResponse> CreateSetupIntent(string customerId = null)
        {
            try
            {
                var options = new SetupIntentCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Usage = "off_session",
                };

                if (!string.IsNullOrEmpty(customerId))
                {
                    options.Customer = customerId;
                }

                var service = new SetupIntentService();
                var setupIntent = await service.CreateAsync(options);

                return new StripeSetupIntentResponse
                {
                    ClientSecret = setupIntent.ClientSecret,
                    CustomerId = setupIntent.CustomerId,
                    SetupIntentId = setupIntent.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating setup intent");
                throw;
            }
        }

        public async Task<Customer> CreateCustomer(StripeCustomerRequest request)
        {
            try
            {
                var options = new CustomerCreateOptions
                {
                    Email = request.Email,
                    Name = request.Name,
                    Phone = request.Phone,
                    PaymentMethod = request.PaymentMethodId,
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = request.PaymentMethodId
                    }
                };

                var service = new CustomerService();
                var customer = await service.CreateAsync(options);

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe customer");
                throw;
            }
        }

        public async Task<PaymentIntent> CreatePaymentIntent(StripePaymentRequest request)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(request.Amount * 100), // Convert to cents
                    Currency = request.Currency,
                    PaymentMethod = request.PaymentMethodId,
                    ConfirmationMethod = "manual",
                    Confirm = true,
                    OffSession = false,
                    Description = request.Description,
                    Metadata = request.Metadata ?? new Dictionary<string, string>(),
                };

                if (!string.IsNullOrEmpty(request.CustomerId))
                {
                    options.Customer = request.CustomerId;
                }

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent: {Message}", ex.Message);

                // Handle specific errors like authentication required
                if (ex.StripeError?.Code == "authentication_required")
                {
                    // Return the payment intent that requires further action
                    var paymentIntentService = new PaymentIntentService();
                    var paymentIntent = await paymentIntentService.GetAsync(ex.StripeError.PaymentIntent?.Id);
                    return paymentIntent;
                }
                throw;
            }
        }

        public async Task<PaymentIntent> ConfirmPaymentIntent(string paymentIntentId, string paymentMethodId = null)
        {
            try
            {
                var options = new PaymentIntentConfirmOptions();

                if (!string.IsNullOrEmpty(paymentMethodId))
                {
                    options.PaymentMethod = paymentMethodId;
                }

                var service = new PaymentIntentService();
                return await service.ConfirmAsync(paymentIntentId, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment intent");
                throw;
            }
        }

        public async Task<PaymentIntent> GetPaymentIntent(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                return await service.GetAsync(paymentIntentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment intent");
                throw;
            }
        }

        public async Task<Refund> CreateRefund(string paymentIntentId, decimal? amount = null)
        {
            try
            {
                var options = new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                };

                if (amount.HasValue)
                {
                    options.Amount = (long?)(amount * 100);
                }

                var service = new RefundService();
                return await service.CreateAsync(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund");
                throw;
            }
        }
    }
}