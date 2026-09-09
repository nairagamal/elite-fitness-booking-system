// Services/PaymentService.cs
using elite.DTOs;
using elite.Interfaces;
using elite.MockGateways;
using elite.Models;

namespace elite.Services
{
    public interface IPaymentService
    {
        Task<CheckoutResponse> CreateCheckoutAsync(CheckoutRequest request);
        Task<PaymentResponse> ProcessPaymentAsync(string checkoutId, PaymentRequest request);
        Task<PaymentResponse> VerifyPaymentAsync(string paymentId);
        Task<PaymentResponse> CancelPaymentAsync(string paymentId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly IEnumerable<IPaymentGateway> _paymentGateways;
        private readonly IMockGatewayFactory _mockGatewayFactory;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            IEnumerable<IPaymentGateway> paymentGateways,
            IMockGatewayFactory mockGatewayFactory,
            ILogger<PaymentService> logger)
        {
            _paymentGateways = paymentGateways;
            _mockGatewayFactory = mockGatewayFactory;
            _logger = logger;
        }

        public async Task<CheckoutResponse> CreateCheckoutAsync(CheckoutRequest request)
        {
            _logger.LogInformation("Creating checkout for order {OrderId} with method {PaymentMethod}",
                request.OrderId, request.PaymentMethod);

            // Check if this is a mock gateway
            if (_mockGatewayFactory.IsMockGateway(request.PaymentMethod))
            {
                return await CreateMockCheckoutAsync(request);
            }

            // Use real payment gateway
            var gateway = GetPaymentGateway(request.PaymentMethod);
            if (gateway == null)
            {
                throw new ArgumentException($"No payment gateway found for method: {request.PaymentMethod}");
            }

            return await gateway.CreateCheckoutAsync(request);
        }

        private async Task<CheckoutResponse> CreateMockCheckoutAsync(CheckoutRequest request)
        {
            var mockGateway = _mockGatewayFactory.GetGateway(request.PaymentMethod);

            var mockRequest = new MockCheckoutRequest
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                Metadata = request.Items
            };

            var mockResponse = await mockGateway.CreateCheckoutAsync(mockRequest);

            // Convert mock response to standard response
            return new CheckoutResponse
            {
                Success = mockResponse.Success,
                Message = mockResponse.Message,
                CheckoutId = mockResponse.CheckoutId,
                CheckoutUrl = mockResponse.CheckoutUrl,
                ExpiresAt = mockResponse.ExpiresAt,
                Metadata = mockResponse.Metadata
            };
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(string checkoutId, PaymentRequest request)
        {
            _logger.LogInformation("Processing payment for checkout {CheckoutId} with method {PaymentMethod}",
                checkoutId, request.PaymentMethod);

            // Check if this is a mock gateway
            if (_mockGatewayFactory.IsMockGateway(request.PaymentMethod))
            {
                return await ProcessMockPaymentAsync(checkoutId, request);
            }

            // Use real payment gateway
            var gateway = GetPaymentGateway(request.PaymentMethod);
            if (gateway == null)
            {
                throw new ArgumentException($"No payment gateway found for method: {request.PaymentMethod}");
            }

            return await gateway.ProcessPaymentAsync(checkoutId, request);
        }

        private async Task<PaymentResponse> ProcessMockPaymentAsync(string checkoutId, PaymentRequest request)
        {
            var mockGateway = _mockGatewayFactory.GetGateway(request.PaymentMethod);

            var mockRequest = new MockPaymentRequest
            {
                OrderId = request.OrderId,
                Amount = request.Amount,
                Currency = request.Currency,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                PaymentDetails = request.Metadata
            };

            var mockResponse = await mockGateway.ProcessPaymentAsync(checkoutId, mockRequest);

            // Convert mock response to standard response
            return new PaymentResponse
            {
                Success = mockResponse.Success,
                Message = mockResponse.Message,
                PaymentId = mockResponse.PaymentId,
                CheckoutId = checkoutId,
                Status = mockResponse.Status,
                Amount = mockResponse.Amount,
                Currency = mockResponse.Currency,
                CreatedAt = mockResponse.CreatedAt,
                ProcessedAt = mockResponse.ProcessedAt,
                Metadata = mockResponse.Metadata
            };
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(string paymentId)
        {
            _logger.LogInformation("Verifying payment {PaymentId}", paymentId);

            // First try mock gateways
            var mockPaymentResponse = await VerifyMockPaymentAsync(paymentId);
            if (mockPaymentResponse != null && mockPaymentResponse.PaymentId != null)
            {
                return mockPaymentResponse;
            }

            // Then try real gateways
            foreach (var gateway in _paymentGateways)
            {
                try
                {
                    var result = await gateway.VerifyPaymentAsync(paymentId);
                    if (result.Success || result.PaymentId == paymentId)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error verifying payment with gateway {GatewayName}", gateway.GatewayName);
                }
            }

            return new PaymentResponse
            {
                Success = false,
                Message = "Payment not found in any gateway",
                Status = "failed"
            };
        }

        private async Task<PaymentResponse> VerifyMockPaymentAsync(string paymentId)
        {
            // Check all mock gateways
            var mockGateways = new[]
            {
                "tabby", "tamara", "creditcard", "credit", "mock"
            };

            foreach (var gatewayName in mockGateways)
            {
                try
                {
                    var mockGateway = _mockGatewayFactory.GetGateway(gatewayName);
                    var mockResponse = await mockGateway.VerifyPaymentAsync(paymentId);

                    if (mockResponse.PaymentId == paymentId || !string.IsNullOrEmpty(mockResponse.PaymentId))
                    {
                        return new PaymentResponse
                        {
                            Success = mockResponse.Success,
                            Message = mockResponse.Message,
                            PaymentId = mockResponse.PaymentId,
                            Status = mockResponse.Status,
                            Amount = mockResponse.Amount,
                            Currency = mockResponse.Currency,
                            CreatedAt = mockResponse.CreatedAt,
                            ProcessedAt = mockResponse.ProcessedAt,
                            Metadata = mockResponse.Metadata
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error verifying payment with mock gateway {GatewayName}", gatewayName);
                }
            }

            return null;
        }

        public async Task<PaymentResponse> CancelPaymentAsync(string paymentId)
        {
            _logger.LogInformation("Cancelling payment {PaymentId}", paymentId);

            // First try mock gateways
            var mockPaymentResponse = await CancelMockPaymentAsync(paymentId);
            if (mockPaymentResponse != null)
            {
                return mockPaymentResponse;
            }

            // Then try real gateways
            foreach (var gateway in _paymentGateways)
            {
                try
                {
                    var result = await gateway.CancelPaymentAsync(paymentId);
                    if (result.PaymentId == paymentId)
                    {
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error cancelling payment with gateway {GatewayName}", gateway.GatewayName);
                }
            }

            return new PaymentResponse
            {
                Success = false,
                Message = "Payment not found in any gateway",
                Status = "failed"
            };
        }

        private async Task<PaymentResponse> CancelMockPaymentAsync(string paymentId)
        {
            // Check all mock gateways
            var mockGateways = new[]
            {
                "tabby", "tamara", "creditcard", "credit", "mock"
            };

            foreach (var gatewayName in mockGateways)
            {
                try
                {
                    var mockGateway = _mockGatewayFactory.GetGateway(gatewayName);
                    var mockResponse = await mockGateway.CancelPaymentAsync(paymentId);

                    if (mockResponse.PaymentId == paymentId)
                    {
                        return new PaymentResponse
                        {
                            Success = mockResponse.Success,
                            Message = mockResponse.Message,
                            PaymentId = mockResponse.PaymentId,
                            Status = mockResponse.Status,
                            Amount = mockResponse.Amount,
                            Currency = mockResponse.Currency,
                            CreatedAt = mockResponse.CreatedAt,
                            ProcessedAt = mockResponse.ProcessedAt,
                            Metadata = mockResponse.Metadata
                        };
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error cancelling payment with mock gateway {GatewayName}", gatewayName);
                }
            }

            return null;
        }

        private IPaymentGateway GetPaymentGateway(string paymentMethod)
        {
            var gateway = _paymentGateways.FirstOrDefault(g => g.SupportsPaymentMethod(paymentMethod));

            // If no real gateway found and it's not a mock gateway, use generic mock
            if (gateway == null && !_mockGatewayFactory.IsMockGateway(paymentMethod))
            {
                _logger.LogWarning("No payment gateway found for method: {PaymentMethod}. Using mock gateway.", paymentMethod);
                gateway = _paymentGateways.FirstOrDefault(g => g.GatewayName == "MockPaymentGateway");
            }

            return gateway;
        }
    }
}