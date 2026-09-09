// MockGateways/TabbyMockGateway.cs
namespace elite.MockGateways
{
    public class TabbyMockGateway : MockGatewayBase
    {
        public override string GatewayName => "Tabby";
        public override decimal SuccessRate => 0.85m; // 85% success rate for Tabby

        public TabbyMockGateway(ILogger<TabbyMockGateway> logger) : base(logger)
        {
        }

        public override async Task<MockCheckoutResponse> CreateCheckoutAsync(MockCheckoutRequest request)
        {
            var baseResponse = await base.CreateCheckoutAsync(request);

            // Add Tabby-specific metadata
            baseResponse.Metadata["installment_options"] = new[]
            {
                new { installments = 4, monthly_payment = request.Amount / 4 },
                new { installments = 6, monthly_payment = request.Amount / 6 },
                new { installments = 12, monthly_payment = request.Amount / 12 }
            };
            baseResponse.Metadata["bnpl_type"] = "installments";

            return baseResponse;
        }
    }
}