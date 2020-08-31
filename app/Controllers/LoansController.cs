using loan_api.Domain;
using loan_api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace loan_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILogger<LoansController> _logger;
        private readonly LoansProvider _loansProvider;

        public LoansController(ILogger<LoansController> logger, LoansProvider loansProvider)
        {
            _logger = logger;
            _loansProvider = loansProvider;
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetAsync(string customerId)
        {
            var loan = _loansProvider.GetLoan(customerId);

            return Ok(loan);
        }
    }
}
