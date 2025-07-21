namespace Meal_Planning.Infrastructure.Services
{
    public interface IDateTimeService
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
        DateTime Today { get; }
    }
    
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
        public DateTime Today => DateTime.Today;
    }
}
