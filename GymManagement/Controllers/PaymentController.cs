using GymManagement.Models;
using GymManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
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

        // Create new payment
        [HttpPost("save")]
        public async Task<IActionResult> SavePayment([FromForm] PaymentRequest request, IFormFile screenshotFile)
        {
            var payment = await _paymentService.SavePaymentAsync(request, screenshotFile);
            return Ok(payment);
        }

        // Update/Edit payment
        [HttpPut("update/{paymentId}")]
        public async Task<IActionResult> UpdatePayment(int paymentId, [FromForm] PaymentRequest request, IFormFile screenshotFile)
        {
            var updatedPayment = await _paymentService.UpdatePaymentAsync(paymentId, request, screenshotFile);
            if (updatedPayment == null)
            {
                return NotFound(new { message = $"Payment with ID {paymentId} not found." });
            }

            return Ok(updatedPayment);
        }

        // Get all payments
        [HttpGet("all")]
        public async Task<IActionResult> GetPayments()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        // Get payments by gym
        [HttpGet("gym-payments")]
        public async Task<ActionResult<List<Payment>>> GetPaymentsByGym([FromQuery] int gymId, [FromQuery] string gymName)
        {
            var payments = await _paymentService.GetPaymentsByGymAsync(gymId, gymName);
            return Ok(payments);
        }

        // Delete payment
        [HttpDelete("{paymentId}")]
        public async Task<IActionResult> DeletePayment(int paymentId)
        {
            var result = await _paymentService.DeletePaymentAsync(paymentId);
            if (!result)
            {
                return NotFound(new { message = $"Payment with ID {paymentId} not found." });
            }

            return Ok(new { message = "Payment deleted successfully." });
        }
    }
}
