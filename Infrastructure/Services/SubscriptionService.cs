using Meal_Planning.Core.Entities;
using Meal_Planning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meal_Planning.Infrastructure.Services
{
    public class SubscriptionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IPaymentService _paymentService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(
            ApplicationDbContext dbContext, 
            IPaymentService paymentService,
            IDateTimeService dateTimeService,
            ILogger<SubscriptionService> logger)
        {
            _dbContext = dbContext;
            _paymentService = paymentService;
            _dateTimeService = dateTimeService;
            _logger = logger;
        }

        public async Task ProcessSubscriptionRenewals()
        {
            try
            {
                // Find subscriptions expiring within the next day
                var now = _dateTimeService.UtcNow;
                var oneDayFromNow = now.AddDays(1);

                var subscriptionsToRenew = await _dbContext.Subscriptions
                    .Where(s => s.Status == "Active" && 
                               s.ExpiresAt.HasValue && 
                               s.ExpiresAt <= oneDayFromNow)
                    .ToListAsync();

                foreach (var subscription in subscriptionsToRenew)
                {
                    try
                    {
                        // Process renewal payment
                        var paymentResult = await _paymentService.ProcessRenewal(
                            subscription.UserId,
                            subscription.PlanId,
                            subscription.Amount);

                        if (paymentResult.Success)
                        {
                            // Update existing subscription with new expiration date
                            subscription.PaymentIntentId = paymentResult.PaymentIntentId;
                            subscription.ExpiresAt = subscription.ExpiresAt?.AddMonths(1) ?? now.AddMonths(1);
                            
                            // Log the successful renewal
                            _logger.LogInformation(
                                "Subscription renewed successfully for user {UserId}, plan {PlanName}", 
                                subscription.UserId, 
                                subscription.PlanName);
                        }
                        else
                        {
                            // Payment failed, mark subscription as inactive
                            subscription.Status = "Payment Failed";
                            subscription.IsActive = false;
                            
                            _logger.LogWarning(
                                "Subscription renewal failed for user {UserId}, plan {PlanName}: {ErrorMessage}", 
                                subscription.UserId, 
                                subscription.PlanName,
                                paymentResult.ErrorMessage);
                            
                            // TODO: Send email to user about failed payment
                        }

                        await _dbContext.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex, 
                            "Error processing subscription renewal for user {UserId}", 
                            subscription.UserId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessSubscriptionRenewals");
            }
        }
    }
}
