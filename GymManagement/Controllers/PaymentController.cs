using GymManagement.Models;
using GymManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace GymManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SavePayment([FromForm] PaymentRequest request, IFormFile screenshotFile)
        {
            var payment = await _paymentService.SavePaymentAsync(request, screenshotFile);
            return Ok(payment);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }
        // ✅ New API
        [HttpGet("gym-payments")]
        public async Task<ActionResult<List<Payment>>> GetPaymentsByGym([FromQuery] int gymId, [FromQuery] string gymName)
        {
            var payments = await _paymentService.GetPaymentsByGymAsync(gymId, gymName);
            return Ok(payments);
        }

    }
}
