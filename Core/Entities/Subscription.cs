// Core/Entities/Subscription.cs
namespace Meal_Planning.Core.Entities
{
    public class Subscription
    {
        public Subscription()
        {
            // Initialize IsActive based on Status in constructor
            IsActive = Status == "Active";
        }
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        
        private string _status = "Pending";
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                IsActive = value == "Active";
            }
        }
        
        public bool IsActive { get; set; } // Regular property instead of computed
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}