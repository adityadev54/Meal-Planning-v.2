// Infrastructure/Services/MockPaymentService.cs
namespace Meal_Planning.Infrastructure.Services
{
    public class MockPaymentService : IPaymentService
    {
        private readonly Dictionary<string, PaymentIntent> _paymentIntents = new();

        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string planId, string planName)
        {
            var paymentIntent = new PaymentIntent
            {
                Id = $"pi_mock_{Guid.NewGuid()}",
                ClientSecret = $"pi_mock_secret_{Guid.NewGuid()}",
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency,
                Metadata = new Dictionary<string, string>
                {
                    { "plan_id", planId },
                    { "plan_name", planName },
                    { "is_mock", "true" }
                }
            };

            _paymentIntents[paymentIntent.Id] = paymentIntent;
            return await Task.FromResult(paymentIntent);
        }

        public async Task ConfirmPaymentAsync(string paymentIntentId)
        {
            if (!_paymentIntents.ContainsKey(paymentIntentId))
            {
                throw new Exception("Invalid payment intent ID");
            }
            // Simulate payment confirmation
            await Task.Delay(500); // Simulate network delay
        }
        
        public async Task<PaymentResult> ProcessRenewal(string userId, string planId, decimal amount)
        {
            // Simulate 95% success rate for renewals
            await Task.Delay(300); // Simulate processing time
            
            bool isSuccess = new Random().Next(100) < 95;
            
            if (isSuccess)
            {
                return new PaymentResult
                {
                    Success = true,
                    PaymentIntentId = $"pi_renewal_{Guid.NewGuid()}",
                    ErrorMessage = ""
                };
            }
            else
            {
                return new PaymentResult
                {
                    Success = false,
                    PaymentIntentId = "",
                    ErrorMessage = "Mock payment failure for testing purposes"
                };
            }
        }
    }
}