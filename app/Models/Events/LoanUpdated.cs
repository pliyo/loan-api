using System;

namespace loan_api.Models.Events
{
    public class LoanUpdated
    {
        public string CustomerId { get; private set; }
        public double DailyUsageAllowance { get; private set; }
        public string CorrelationId { get; private set; }
        public DateTime EventDate { get; private set; }
        
        public LoanUpdated(string customerId, double dailyUsageAllowance, string correlationId)
        {
            CustomerId = customerId;
            DailyUsageAllowance = dailyUsageAllowance;
            CorrelationId = correlationId;
            EventDate = DateTime.UtcNow;
        }
    }
}
