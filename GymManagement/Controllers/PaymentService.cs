using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly string key = "rzp_live_RIIy4KDqcIQPqh";      // Razorpay Key ID
    private readonly string secret = "If9sUD1c76NoJCbobRdZJt20";   // Razorpay Secret Key

    // ✅ Create Razorpay Order
    [HttpPost("create-order")]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        RazorpayClient client = new RazorpayClient(key, secret);

        var options = new Dictionary<string, object>
        {
            { "amount", request.Amount * 100 }, // in paise
            { "currency", "INR" },
            { "receipt", "receipt_" + Guid.NewGuid().ToString("N").Substring(0, 32) } // <=40 chars
        };

        Order order = client.Order.Create(options);

        // Return only minimal data needed by Angular frontend
        return Ok(new
        {
            orderId = order["id"].ToString(),
            amount = order["amount"],
            currency = order["currency"]
        });
    }

    // ✅ Verify Razorpay Payment
    [HttpPost("verify-payment")]
    public IActionResult VerifyPayment([FromBody] RazorpayPaymentRequest data)
    {
        if (data == null || string.IsNullOrEmpty(data.RazorpayOrderId) || string.IsNullOrEmpty(data.RazorpayPaymentId) || string.IsNullOrEmpty(data.RazorpaySignature))
        {
            return BadRequest(new { success = false, message = "Missing required fields" });
        }

        try
        {
            string generatedSignature = RazorpayUtils.CalculateSignature(
                data.RazorpayOrderId + "|" + data.RazorpayPaymentId, secret);

            bool isValid = generatedSignature == data.RazorpaySignature;

            return Ok(new { success = isValid });
        }
        catch
        {
            return BadRequest(new { success = false });
        }
    }
}

// ✅ Razorpay signature utility
public static class RazorpayUtils
{
    public static string CalculateSignature(string message, string secret)
    {
        var encoding = new UTF8Encoding();
        byte[] keyBytes = encoding.GetBytes(secret);
        byte[] messageBytes = encoding.GetBytes(message);

        using (var hmacsha256 = new HMACSHA256(keyBytes))
        {
            byte[] hashMessage = hmacsha256.ComputeHash(messageBytes);
            return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
        }
    }
}

// ✅ Models
public class OrderRequest
{
    public int Amount { get; set; } // In rupees
}

public class RazorpayPaymentRequest
{
    public string RazorpayOrderId { get; set; }
    public string RazorpayPaymentId { get; set; }
    public string RazorpaySignature { get; set; }
}
