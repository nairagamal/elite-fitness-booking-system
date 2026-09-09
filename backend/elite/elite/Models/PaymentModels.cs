// Models/PaymentModels.cs
namespace elite.Models
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string PaymentMethod { get; set; } // "mock", "tabby", "tamara", "mada", "applepay", "creditcard"
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string CheckoutId { get; set; }
        public string PaymentId { get; set; }
        public string Status { get; set; } // "pending", "success", "failed", "cancelled"
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class CheckoutRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SAR";
        public string PaymentMethod { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string ReturnUrl { get; set; }
        public string CancelUrl { get; set; }
        public Dictionary<string, object> Items { get; set; } = new();
    }

    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string CheckoutId { get; set; }
        public string CheckoutUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}