// MockGateways/MockGatewayFactory.cs
using Microsoft.Extensions.DependencyInjection;

namespace elite.MockGateways
{
    public interface IMockGatewayFactory
    {
        IMockGateway GetGateway(string gatewayName);
        bool IsMockGateway(string gatewayName);
    }

    public class MockGatewayFactory : IMockGatewayFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _gatewayTypes;

        public MockGatewayFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _gatewayTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "tabby", typeof(TabbyMockGateway) },
                { "tamara", typeof(TamaraMockGateway) },
                { "creditcard", typeof(CreditCardMockGateway) },
                { "credit", typeof(CreditCardMockGateway) },
                { "mock", typeof(MockPaymentGatewayWrapper) } // Use wrapper instead
            };
        }

        public IMockGateway GetGateway(string gatewayName)
        {
            if (string.IsNullOrEmpty(gatewayName))
                throw new ArgumentException("Gateway name is required");

            var normalizedName = gatewayName.ToLower();

            if (!_gatewayTypes.TryGetValue(normalizedName, out var gatewayType))
            {
                // Fallback to generic mock gateway
                return _serviceProvider.GetRequiredService<MockPaymentGatewayWrapper>();
            }

            return (IMockGateway)_serviceProvider.GetRequiredService(gatewayType);
        }

        public bool IsMockGateway(string gatewayName)
        {
            if (string.IsNullOrEmpty(gatewayName))
                return false;

            var normalizedName = gatewayName.ToLower();
            return _gatewayTypes.ContainsKey(normalizedName) || normalizedName == "mock";
        }
    }
}