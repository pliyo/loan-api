using loan_api.Models;
using loan_api.Models.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace loan_api.Domain
{
    /// <summary>
    /// Assume data is in memory as we don't want to complicate this exercise with a real DB
    /// We'll have two loans, one that is already paid, and one that it's not
    /// </summary>
    public class LoansProvider
    {
        private Dictionary<string, Loan> _loans;
        private readonly IEventSender _eventSender;
        
        public LoansProvider(IEventSender eventSender)
        {
            var firstCustomerId = "2516e61e-8da9-4a22-9a70-f8b9c01bacca";
            var firstLoan = new Loan()
            {
                CustomerId = firstCustomerId,
                DailyRate = 5,
                ProductId = "Home Solar System Energy Plus",
                ShopId = "4 Privet Drive",
                TotalLoanAmount = 200,
                TotalLoanPayed = 100,
                CreationDate = DateTime.UtcNow.AddDays(-3),
            };
            var secondCustomerId = "00999b75-6924-47d4-b9e9-a8a0b25ae8d6";
            var secondLoan = new Loan()
            {
                CustomerId = secondCustomerId,
                DailyRate = 70,
                ProductId = "Home Solar System 2000+",
                ShopId = "Diagon Alley",
                TotalLoanAmount = 7000,
                TotalLoanPayed = 7000,
                CreationDate = DateTime.UtcNow.AddDays(-10),
            };
            _loans = new Dictionary<string, Loan>
            {
                { firstLoan.CustomerId, firstLoan },
                { secondLoan.CustomerId, secondLoan }
            };
            _eventSender = eventSender;
        }

        /// Updates Loan
        /// Assume deduplication of messages is handled somewhere else
        /// ASK PM: What do we do with Payments received for loans that don't exists or are inactive / fully paid?
        public async Task<Loan> UpdateLoanAsync(PaymentReceived paymentReceived)
        {
            if(ActiveLoanExists(paymentReceived.CustomerId))
            {
                var loan = GetLoan(paymentReceived.CustomerId);

                loan.UpdateTotalLoanPayed(paymentReceived.PaymentAmount);
                _loans[loan.CustomerId] = loan;
                
                if(loan.State() == LoanState.FINISHED)
                {
                    var loanFinished = new LoanFinished(loan.CustomerId, paymentReceived.CorrelationId);
                    await _eventSender.SendLoanFinishedAsync(loanFinished);
                }
                else
                {
                    var loanUpdated = new LoanUpdated(paymentReceived.CustomerId, loan.DailyUsageAllowance(), paymentReceived.CorrelationId);
                    await _eventSender.SendLoanUpdatedAsync(loanUpdated);
                }

                return loan;
            }

            return null;
        }

        public Loan GetLoan(string customerId)
        {
            var loan = new Loan();
            _loans.TryGetValue(customerId, out loan);
            return loan;
        }

        public bool ActiveLoanExists(string customerId)
        {
            if (_loans.ContainsKey(customerId) && _loans[customerId].State() != LoanState.FINISHED)
                return true;
            return false;
        }
    }
}
