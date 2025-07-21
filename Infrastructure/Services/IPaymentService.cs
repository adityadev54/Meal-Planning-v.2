// Infrastructure/Services/IPaymentService.cs
namespace Meal_Planning.Infrastructure.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string planId, string planName);
        Task ConfirmPaymentAsync(string paymentIntentId);
        Task<PaymentResult> ProcessRenewal(string userId, string planId, decimal amount);
    }

    public class PaymentIntent
    {
        public string Id { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
    
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}