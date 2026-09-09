// MockGateways/IMockGateway.cs
namespace elite.MockGateways
{
    public interface IMockGateway
    {
        string GatewayName { get; }
        Task<MockCheckoutResponse> CreateCheckoutAsync(MockCheckoutRequest request);
        Task<MockPaymentResponse> ProcessPaymentAsync(string checkoutId, MockPaymentRequest request);
        Task<MockPaymentResponse> VerifyPaymentAsync(string paymentId);
        Task<MockPaymentResponse> CancelPaymentAsync(string paymentId);
    }

    public class MockCheckoutRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class MockCheckoutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string CheckoutId { get; set; }
        public string CheckoutUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class MockPaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public Dictionary<string, object> PaymentDetails { get; set; } = new();
    }

    public class MockPaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}