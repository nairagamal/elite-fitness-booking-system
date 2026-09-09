// MockGateways/MockPaymentGatewayWrapper.cs
using elite.Services;

namespace elite.MockGateways
{
    public class MockPaymentGatewayWrapper : MockGatewayBase
    {
        public override string GatewayName => "MockPayment";
        public override decimal SuccessRate => 0.8m;

        public MockPaymentGatewayWrapper(ILogger<MockPaymentGatewayWrapper> logger) : base(logger)
        {
        }
    }
}