using loan_api.Models.Events;
using System.Threading.Tasks;

namespace loan_api.Domain
{
    public interface IEventSender
    {
        Task SendLoanUpdatedAsync(LoanUpdated loanUpdated);
        Task SendLoanFinishedAsync(LoanFinished loanFinished);
    }
}
