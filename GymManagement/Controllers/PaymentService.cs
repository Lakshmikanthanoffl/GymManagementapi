using GymManagement.Interfaces;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Data;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly string key = "rzp_test_RIYosIVFWWyoSn";      // Razorpay Key ID
    private readonly string secret = "mHlGfXgXSjjhZ5zAYll35XY2"; // Razorpay Secret Key
    private readonly IRoleRepository _roleRepository;

    public PaymentController(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    // ✅ Create Razorpay Order
    [HttpPost("create-order")]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        RazorpayClient client = new RazorpayClient(key, secret);

        var options = new Dictionary<string, object>
        {
            { "amount", request.Amount * 100 }, // amount in paise
            { "currency", "INR" },
            { "receipt", "receipt_" + Guid.NewGuid().ToString("N").Substring(0, 32) } // <=40 chars
        };

        Order order = client.Order.Create(options);

        return Ok(new
        {
            orderId = order["id"].ToString(),
            amount = order["amount"],
            currency = order["currency"]
        });
    }

    // ✅ Verify Razorpay Payment & Update Subscription
    [HttpPost("verify-payment")]
    public async Task<IActionResult> VerifyPayment([FromBody] RazorpayPaymentRequest data)
    {
        if (data == null || string.IsNullOrEmpty(data.RazorpayOrderId) ||
            string.IsNullOrEmpty(data.RazorpayPaymentId) || string.IsNullOrEmpty(data.RazorpaySignature))
        {
            return BadRequest(new { success = false, message = "Missing required fields" });
        }

        // Declare role before so it’s available outside
        Role role = null;

        try
        {
            // Verify Razorpay signature
            string generatedSignature = RazorpayUtils.CalculateSignature(
                data.RazorpayOrderId + "|" + data.RazorpayPaymentId, secret);

            bool isValid = generatedSignature == data.RazorpaySignature;

            if (isValid)
            {
                // ✅ Update subscription for the user
                role = await _roleRepository.GetRoleByIdAsync(data.RoleId);
                if (role != null)
                {
                    // Always use UTC
                    role.PaidDate = DateTime.UtcNow;

                    // Extend subscription
                    switch (data.PlanName)
                    {
                        case "Month":
                            role.ValidUntil = role.ValidUntil != null && role.ValidUntil > DateTime.UtcNow
                                ? role.ValidUntil.Value.ToUniversalTime().AddMonths(1)
                                : DateTime.UtcNow.AddMonths(1);
                            break;

                        case "3 Months":
                            role.ValidUntil = role.ValidUntil != null && role.ValidUntil > DateTime.UtcNow
                                ? role.ValidUntil.Value.ToUniversalTime().AddMonths(3)
                                : DateTime.UtcNow.AddMonths(3);
                            break;

                        case "Year":
                            role.ValidUntil = role.ValidUntil != null && role.ValidUntil > DateTime.UtcNow
                                ? role.ValidUntil.Value.ToUniversalTime().AddYears(1)
                                : DateTime.UtcNow.AddYears(1);
                            break;

                        default:
                            role.ValidUntil = DateTime.UtcNow.AddMonths(1);
                            break;
                    }

                    role.AmountPaid = data.Amount;
                    role.IsActive = true;
                    
                    // ✅ Update privileges
                    if (data.Privileges != null && data.Privileges.Count > 0)
                    {
                        role.Privileges = data.Privileges; // already exists in DB
                    }


                    await _roleRepository.UpdateRoleAsync(role);
                }
            }

            return Ok(new { success = isValid, role });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
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
    public int RoleId { get; set; }        // Role/User to update
    public decimal Amount { get; set; }    // Amount paid
    public string PlanName { get; set; }   // Subscription Plan: Monthly, 3 Months, Yearly

    public List<string> Privileges { get; set; }
}
