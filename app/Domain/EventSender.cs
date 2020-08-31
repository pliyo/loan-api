using loan_api.Models.Events;
using System;
using System.Threading.Tasks;

namespace loan_api.Domain
{
    public class EventSender : IEventSender
    {
        public Task SendLoanFinishedAsync(LoanFinished loanFinished)
        {
            throw new NotImplementedException();
        }

        public Task SendLoanUpdatedAsync(LoanUpdated loanUpdated)
        {
            throw new NotImplementedException();
        }
    }
}
