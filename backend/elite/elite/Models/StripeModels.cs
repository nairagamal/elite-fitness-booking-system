using System.Text.Json.Serialization;

namespace elite.Models
{
    public class StripePaymentRequest
    {
        public string PaymentMethodId { get; set; }
        public string PaymentIntentId { get; set; }
        public string CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "sar";
        public string Description { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public class StripeCustomerRequest
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string PaymentMethodId { get; set; }
    }

    public class StripeSetupIntentResponse
    {
        public string ClientSecret { get; set; }
        public string CustomerId { get; set; }
        public string SetupIntentId { get; set; }
    }
}