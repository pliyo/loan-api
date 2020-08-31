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

        public LoansController(ILogger<LoansController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            _logger.LogInformation("Get...");
            return Ok();
        }
    }
}
