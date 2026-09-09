// Services/MockPaymentGateway.cs
using elite.DTOs;
using elite.Interfaces;
using elite.Models;

namespace elite.Services
{
    public class MockPaymentGateway : IPaymentGateway
    {
        private readonly ILogger<MockPaymentGateway> _logger;
        private readonly Dictionary<string, CheckoutRequest> _checkouts;
        private readonly Dictionary<string, PaymentResponse> _payments;

        public string GatewayName => "MockPaymentGateway";

        public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
        {
            _logger = logger;
            _checkouts = new Dictionary<string, CheckoutRequest>();
            _payments = new Dictionary<string, PaymentResponse>();
        }

        // In the MockPaymentGateway.cs - Update the CreateCheckoutAsync method:
        public async Task<CheckoutResponse> CreateCheckoutAsync(CheckoutRequest request)
        {
            _logger.LogInformation("Creating mock checkout for order {OrderId}", request.OrderId);

            // Simulate API delay
            await Task.Delay(500);

            var checkoutId = $"mock_checkout_{Guid.NewGuid():N}";

            // Use the mock payment controller route
            var checkoutUrl = $"/mock-payment/checkout/{checkoutId}";

            _checkouts[checkoutId] = request;

            var response = new CheckoutResponse
            {
                Success = true,
                Message = "Checkout created successfully",
                CheckoutId = checkoutId,
                CheckoutUrl = checkoutUrl,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Metadata = new Dictionary<string, object>
        {
            { "mock_data", "This is simulated checkout data" },
            { "test_mode", true },
            { "gateway", GatewayName },
            { "amount", request.Amount },
            { "currency", request.Currency }
        }
            };

            _logger.LogInformation("Mock checkout created: {CheckoutId}", checkoutId);
            return response;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(string checkoutId, PaymentRequest request)
        {
            _logger.LogInformation("Processing mock payment for checkout {CheckoutId}", checkoutId);

            // Simulate API delay
            await Task.Delay(800);

            if (!_checkouts.ContainsKey(checkoutId))
            {
                return new PaymentResponse
                {
                    Success = false,
                    Message = "Checkout not found or expired",
                    Status = "failed"
                };
            }

            var checkout = _checkouts[checkoutId];

            // Simulate random payment outcomes for testing
            var random = new Random();
            var successRate = 0.8; // 80% success rate for testing

            var paymentId = $"mock_payment_{Guid.NewGuid():N}";
            var isSuccess = random.NextDouble() < successRate;

            var response = new PaymentResponse
            {
                Success = isSuccess,
                Message = isSuccess ? "Payment processed successfully" : "Payment failed - simulated decline",
                PaymentId = paymentId,
                CheckoutId = checkoutId,
                Status = isSuccess ? "success" : "failed",
                Amount = request.Amount,
                Currency = request.Currency,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "mock_transaction_id", Guid.NewGuid().ToString() },
                    { "test_mode", true },
                    { "gateway", GatewayName },
                    { "simulated_result", isSuccess ? "approved" : "declined" }
                }
            };

            _payments[paymentId] = response;
            _checkouts.Remove(checkoutId); // Checkout is consumed

            _logger.LogInformation("Mock payment processed: {PaymentId}, Success: {Success}", paymentId, isSuccess);
            return response;
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(string paymentId)
        {
            _logger.LogInformation("Verifying mock payment {PaymentId}", paymentId);

            // Simulate API delay
            await Task.Delay(300);

            if (_payments.TryGetValue(paymentId, out var payment))
            {
                _logger.LogInformation("Payment verification successful for {PaymentId}", paymentId);
                return payment;
            }

            _logger.LogWarning("Payment not found for verification: {PaymentId}", paymentId);
            return new PaymentResponse
            {
                Success = false,
                Message = "Payment not found",
                Status = "failed"
            };
        }

        public async Task<PaymentResponse> CancelPaymentAsync(string paymentId)
        {
            _logger.LogInformation("Cancelling mock payment {PaymentId}", paymentId);

            // Simulate API delay
            await Task.Delay(400);

            if (_payments.TryGetValue(paymentId, out var payment))
            {
                payment.Status = "cancelled";
                payment.Success = false;
                payment.Message = "Payment cancelled successfully";
                payment.ProcessedAt = DateTime.UtcNow;

                _logger.LogInformation("Payment cancelled successfully: {PaymentId}", paymentId);
                return payment;
            }

            return new PaymentResponse
            {
                Success = false,
                Message = "Payment not found",
                Status = "failed"
            };
        }

        public bool SupportsPaymentMethod(string paymentMethod)
        {
            // Mock gateway supports all payment methods for testing
            return true;
        }
    }
}