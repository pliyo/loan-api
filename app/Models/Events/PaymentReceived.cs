using System;

namespace loan_api.Models.Events
{
    public class PaymentReceived
    {
        public string PaymentId { get; private set; }
        public string CustomerId { get; set; }
        public string PhoneNumber { get; set; }
        public double PaymentAmount { get; set; }
        public string CorrelationId { get; private set; }
        public DateTime EventDate { get; private set; }

        /// <summary>
        /// PaymentId, CorrelationId and EventDate will come from the event.
        /// We are generating them here only to add context to this exercise
        /// </summary>
        public PaymentReceived()
        {
            PaymentId = Guid.NewGuid().ToString();
            CorrelationId = Guid.NewGuid().ToString();
            EventDate = DateTime.UtcNow;
        }
    }
}
