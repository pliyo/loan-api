using FluentAssertions;
using loan_api.Domain;
using loan_api.Models;
using loan_api.Models.Events;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace loan_api_tests
{
    public class LoanTests
    {
        private readonly LoansProvider _loansProvider;
        private const string EXISTING_CUSTOMER_ID = "2516e61e-8da9-4a22-9a70-f8b9c01bacca";
        private readonly Mock<IEventSender> _eventSender;

        public LoanTests()
        {
            _eventSender = new Mock<IEventSender>();
            _loansProvider = new LoansProvider(_eventSender.Object);
        }

        [Fact]
        public async Task UpdateLoan_Sends_LoanUptadedEvent_When_LoanIs_Unfinished()
        {
            var initialLoan = _loansProvider.GetLoan(EXISTING_CUSTOMER_ID);

            var paymentReceived = new PaymentReceived()
            {
                CustomerId = EXISTING_CUSTOMER_ID,
                PaymentAmount = initialLoan.RemainingAmountToPay() - 10,
            };

            var expectedLoan = await _loansProvider.UpdateLoanAsync(paymentReceived);

            _eventSender.Verify(y => y.SendLoanUpdatedAsync(It.IsAny<LoanUpdated>()), Times.Once);
            _eventSender.Verify(y => y.SendLoanFinishedAsync(It.IsAny<LoanFinished>()), Times.Never);
        }

        [Fact]
        public async Task UpdateLoan_Sends_LoanFinishedEvent_When_LoanIs_Finished()
        {
            var initialLoan = _loansProvider.GetLoan(EXISTING_CUSTOMER_ID);

            var paymentReceived = new PaymentReceived()
            {
                CustomerId = EXISTING_CUSTOMER_ID,
                PaymentAmount = initialLoan.RemainingAmountToPay(),
            };

            var expectedLoan = await _loansProvider.UpdateLoanAsync(paymentReceived);

            _eventSender.Verify(y => y.SendLoanUpdatedAsync(It.IsAny<LoanUpdated>()), Times.Never);
            _eventSender.Verify(y => y.SendLoanFinishedAsync(It.IsAny<LoanFinished>()), Times.Once);
        }

        [Fact]
        public async Task UpdateLoan_When_PaymentsComes_With_AllRemainingAmount_Sets_Loan_As_Finished()
        {
            var initialLoan = _loansProvider.GetLoan(EXISTING_CUSTOMER_ID);

            initialLoan.State().Should().Be(LoanState.UNFINISHED);
            initialLoan.TotalLoanPayed.Should().BeLessThan(initialLoan.TotalLoanAmount);

            var paymentReceived = new PaymentReceived()
            {
                CustomerId = EXISTING_CUSTOMER_ID,
                PaymentAmount = initialLoan.RemainingAmountToPay(),
            };

            var expectedLoan = await _loansProvider.UpdateLoanAsync(paymentReceived);
            
            expectedLoan.State().Should().Be(LoanState.FINISHED);
            expectedLoan.TotalLoanPayed.Should().Be(expectedLoan.TotalLoanAmount);
            expectedLoan.CustomerId.Should().Be(paymentReceived.CustomerId);
        }

        [Fact]
        public async Task UpdateLoan_When_PaymentsComes_With_Not_AllRemainingAmount_Sets_Loan_As_Unfinished()
        {
            var initialLoan = _loansProvider.GetLoan(EXISTING_CUSTOMER_ID);

            initialLoan.State().Should().Be(LoanState.UNFINISHED);
            initialLoan.TotalLoanPayed.Should().BeLessThan(initialLoan.TotalLoanAmount);

            var paymentReceived = new PaymentReceived()
            {
                CustomerId = EXISTING_CUSTOMER_ID,
                PaymentAmount = initialLoan.RemainingAmountToPay() - 1,
            };

            var expectedLoan = await _loansProvider.UpdateLoanAsync(paymentReceived);

            expectedLoan.State().Should().Be(LoanState.UNFINISHED);
            expectedLoan.TotalLoanPayed.Should().BeLessThan(expectedLoan.TotalLoanAmount);
            expectedLoan.CustomerId.Should().Be(paymentReceived.CustomerId);
        }

        [Fact]
        public async Task DailyUsageAllowance_IsUpdated_EveryTime_A_New_PaymentReceived_Comes()
        {
            var initialLoan = _loansProvider.GetLoan(EXISTING_CUSTOMER_ID);

            var payingHalfOfWhatsLeft = initialLoan.RemainingAmountToPay() / 2;
            var firstPayment = new PaymentReceived()
            {
                CustomerId = EXISTING_CUSTOMER_ID,
                PaymentAmount = payingHalfOfWhatsLeft,
            };

            var loanAfterFirstPayment = await _loansProvider.UpdateLoanAsync(firstPayment);

            var dailyAllowance = loanAfterFirstPayment.DailyUsageAllowance();
            dailyAllowance.Should().Be(loanAfterFirstPayment.TotalLoanPayed / loanAfterFirstPayment.DailyRate);
            dailyAllowance.Should().BeGreaterThan(0);

            var secondPayment = new PaymentReceived()
            {
                CustomerId = EXISTING_CUSTOMER_ID,
                PaymentAmount = payingHalfOfWhatsLeft,
            };

            var loanAfterLastPayment = await _loansProvider.UpdateLoanAsync(secondPayment);

            var updatedDailyAllowance = loanAfterLastPayment.DailyUsageAllowance();
            updatedDailyAllowance.Should().BeGreaterThan(dailyAllowance);
            updatedDailyAllowance.Should().Be(loanAfterFirstPayment.TotalLoanPayed / loanAfterFirstPayment.DailyRate);
            updatedDailyAllowance.Should().BeGreaterThan(0);
            loanAfterFirstPayment.State().Should().Be(LoanState.FINISHED);
        }

        [Fact]
        public void GetLoan_For_An_Existing_Customer()
        {
            var loan = _loansProvider.GetLoan(EXISTING_CUSTOMER_ID);

            loan.CustomerId.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void GetLoan_For_A_Non_Existing_Customer()
        {
            var randomCustomerId = Guid.NewGuid().ToString();

            var loan = _loansProvider.GetLoan(randomCustomerId);

            loan.Should().BeNull();
        }

        [Fact]
        public void UpdateTotalLoanPayed_With_NewAmount()
        {
            var initialPayment = 10;
            var loan = new Loan()
            {
                TotalLoanAmount = 1000,
                TotalLoanPayed = initialPayment
            };

            var paymentAmount = 100;
            loan.UpdateTotalLoanPayed(paymentAmount);

            loan.TotalLoanPayed.Should().Be(initialPayment + paymentAmount);
        }

        [Theory]
        [InlineData("2516e61e-8da9-4a22-9a70-f8b9c01bacca", true)]
        [InlineData("00999b75-6924-47d4-b9e9-a8a0b25ae8d6", false)]
        [InlineData("55555555-5555-5555-5555-555555555555", false)]
        public void ActiveLoan_Fetches_If_TheLoanIsActive(string customerId, bool isActive)
        {
            _loansProvider.ActiveLoanExists(customerId).Should().Be(isActive);
        }
    }
}
