// Interfaces/IPaymentGateway.cs
using elite.DTOs;
using elite.Models;

namespace elite.Interfaces
{
    public interface IPaymentGateway
    {
        string GatewayName { get; }
        Task<CheckoutResponse> CreateCheckoutAsync(CheckoutRequest request);
        Task<PaymentResponse> ProcessPaymentAsync(string checkoutId, PaymentRequest request);
        Task<PaymentResponse> VerifyPaymentAsync(string paymentId);
        Task<PaymentResponse> CancelPaymentAsync(string paymentId);
        bool SupportsPaymentMethod(string paymentMethod);
    }
}