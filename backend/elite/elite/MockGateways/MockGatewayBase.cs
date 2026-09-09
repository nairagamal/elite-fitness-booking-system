// MockGateways/MockGatewayBase.cs
namespace elite.MockGateways
{
    public abstract class MockGatewayBase : IMockGateway
    {
        protected readonly ILogger _logger;
        protected readonly Dictionary<string, MockCheckoutRequest> _checkouts;
        protected readonly Dictionary<string, MockPaymentResponse> _payments;

        public abstract string GatewayName { get; }
        public abstract decimal SuccessRate { get; }

        protected MockGatewayBase(ILogger logger)
        {
            _logger = logger;
            _checkouts = new Dictionary<string, MockCheckoutRequest>();
            _payments = new Dictionary<string, MockPaymentResponse>();
        }

        public virtual async Task<MockCheckoutResponse> CreateCheckoutAsync(MockCheckoutRequest request)
        {
            await Task.Delay(500); // Simulate API delay

            var checkoutId = $"{GatewayName.ToLower()}_checkout_{Guid.NewGuid():N}";
            var checkoutUrl = $"/mock-gateway/{GatewayName.ToLower()}/checkout/{checkoutId}";

            _checkouts[checkoutId] = request;

            return new MockCheckoutResponse
            {
                Success = true,
                Message = $"{GatewayName} checkout created successfully",
                CheckoutId = checkoutId,
                CheckoutUrl = checkoutUrl,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Metadata = new Dictionary<string, object>
                {
                    { "gateway", GatewayName },
                    { "test_mode", true },
                    { "amount", request.Amount },
                    { "currency", request.Currency }
                }
            };
        }

        public virtual async Task<MockPaymentResponse> ProcessPaymentAsync(string checkoutId, MockPaymentRequest request)
        {
            await Task.Delay(800); // Simulate API delay

            if (!_checkouts.ContainsKey(checkoutId))
            {
                return new MockPaymentResponse
                {
                    Success = false,
                    Message = "Checkout not found or expired",
                    Status = "failed"
                };
            }

            var random = new Random();
            var randomValue = (decimal)random.NextDouble(); // Fixed: Convert to decimal
            var isSuccess = randomValue < SuccessRate;

            var paymentId = $"{GatewayName.ToLower()}_payment_{Guid.NewGuid():N}";

            var response = new MockPaymentResponse
            {
                Success = isSuccess,
                Message = isSuccess ? "Payment processed successfully" : "Payment failed",
                PaymentId = paymentId,
                Status = isSuccess ? "success" : "failed",
                Amount = request.Amount,
                Currency = request.Currency,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    { "gateway", GatewayName },
                    { "test_mode", true },
                    { "simulated_result", isSuccess ? "approved" : "declined" }
                }
            };

            _payments[paymentId] = response;
            _checkouts.Remove(checkoutId);

            return response;
        }

        public virtual async Task<MockPaymentResponse> VerifyPaymentAsync(string paymentId)
        {
            await Task.Delay(300);

            if (_payments.TryGetValue(paymentId, out var payment))
            {
                return payment;
            }

            return new MockPaymentResponse
            {
                Success = false,
                Message = "Payment not found",
                Status = "failed"
            };
        }

        public virtual async Task<MockPaymentResponse> CancelPaymentAsync(string paymentId)
        {
            await Task.Delay(400);

            if (_payments.TryGetValue(paymentId, out var payment))
            {
                payment.Status = "cancelled";
                payment.Success = false;
                payment.Message = "Payment cancelled successfully";
                payment.ProcessedAt = DateTime.UtcNow;

                return payment;
            }

            return new MockPaymentResponse
            {
                Success = false,
                Message = "Payment not found",
                Status = "failed"
            };
        }
    }
}