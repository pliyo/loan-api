using System;

namespace loan_api.Models
{
    public class Loan
    {
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
        public string ShopId { get; set; }
        public double TotalLoanAmount { get; set; }
        public double TotalLoanPayed { get; set; } // Assume we are going to keep track of how much you have payed already
        public double DailyRate { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public LoanState State()
        {
            if (TotalLoanPayed == TotalLoanAmount)
                return LoanState.FINISHED;
            return LoanState.UNFINISHED;
        }

        public double DailyUsageAllowance()
        {
            return Math.Ceiling(TotalLoanPayed / DailyRate);
        }

        public double RemainingAmountToPay()
        {
            return TotalLoanAmount - TotalLoanPayed;
        }

        public void UpdateTotalLoanPayed(double paymentAmount)
        {
            LastUpdated = DateTime.UtcNow;
            TotalLoanPayed += paymentAmount;
        }
    }

    public enum LoanState
    {
        FINISHED,
        UNFINISHED
    }
}
