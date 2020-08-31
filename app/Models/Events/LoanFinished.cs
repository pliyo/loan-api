using System;

namespace loan_api.Models.Events
{
    public class LoanFinished
    {
        public string CustomerId { get; private set; }
        public string CorrelationId { get; private set; }
        public DateTime EventDate { get; private set; }
        
        public LoanFinished(string customerId, string correlationId)
        {
            CustomerId = customerId;
            CorrelationId = correlationId;
            EventDate = DateTime.UtcNow;
        }
    }
}
