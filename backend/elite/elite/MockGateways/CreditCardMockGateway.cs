// MockGateways/CreditCardMockGateway.cs
namespace elite.MockGateways
{
    public class CreditCardMockGateway : MockGatewayBase
    {
        public override string GatewayName => "CreditCard";
        public override decimal SuccessRate => 0.95m; // 95% success rate for Credit Card

        public CreditCardMockGateway(ILogger<CreditCardMockGateway> logger) : base(logger)
        {
        }

        public override async Task<MockCheckoutResponse> CreateCheckoutAsync(MockCheckoutRequest request)
        {
            var baseResponse = await base.CreateCheckoutAsync(request);

            // Add Credit Card specific metadata
            baseResponse.Metadata["card_types"] = new[] { "Visa", "MasterCard", "American Express" };
            baseResponse.Metadata["requires_cvv"] = true;
            baseResponse.Metadata["3d_secure"] = true;

            return baseResponse;
        }

        public override async Task<MockPaymentResponse> ProcessPaymentAsync(string checkoutId, MockPaymentRequest request)
        {
            var response = await base.ProcessPaymentAsync(checkoutId, request);

            // Simulate different failure reasons for credit card
            if (!response.Success)
            {
                var reasons = new[]
                {
                    "Insufficient funds",
                    "Card expired",
                    "Invalid CVV",
                    "3D Secure authentication failed",
                    "Daily limit exceeded"
                };

                var random = new Random();
                response.Message = $"Card payment failed: {reasons[random.Next(reasons.Length)]}";

                response.Metadata["failure_reason"] = response.Message;
                response.Metadata["suggested_action"] = "Try a different payment method or contact your bank";
            }

            return response;
        }
    }
}