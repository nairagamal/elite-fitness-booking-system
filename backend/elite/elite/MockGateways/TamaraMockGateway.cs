// MockGateways/TamaraMockGateway.cs
namespace elite.MockGateways
{
    public class TamaraMockGateway : MockGatewayBase
    {
        public override string GatewayName => "Tamara";
        public override decimal SuccessRate => 0.90m; // 90% success rate for Tamara

        public TamaraMockGateway(ILogger<TamaraMockGateway> logger) : base(logger)
        {
        }

        public override async Task<MockCheckoutResponse> CreateCheckoutAsync(MockCheckoutRequest request)
        {
            var baseResponse = await base.CreateCheckoutAsync(request);

            // Add Tamara-specific metadata
            baseResponse.Metadata["payment_options"] = new[]
            {
                new { name = "Pay in 30 days", description = "Pay after 30 days" },
                new { name = "Pay in 3 installments", description = "Split into 3 payments" }
            };
            baseResponse.Metadata["merchant_url"] = "https://merchant.example.com";

            return baseResponse;
        }
    }
}