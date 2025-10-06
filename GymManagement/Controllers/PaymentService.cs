using GymManagement.Interfaces;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly string key = "rzp_test_RIYosIVFWWyoSn";      // Razorpay Key ID
    private readonly string secret = "mHlGfXgXSjjhZ5zAYll35XY2"; // Razorpay Secret Key
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailService _emailService;
    private readonly IInvoiceService _invoiceService; // Optional, if you generate PDFs

    // Subscription plans
    private readonly List<SubscriptionPlan> subscriptionPlans = new()
    {
        new SubscriptionPlan
        {
            Name = "Basic",
            Monthly = 7999,
            Quarterly = (int)Math.Round(7999 * 3 * 0.9),
            Yearly = (int)Math.Round(7999 * 12 * 0.8)
        },
        new SubscriptionPlan
        {
            Name = "Advanced",
            Monthly = 10000,
            Quarterly = (int)Math.Round(10000 * 3 * 0.9),
            Yearly = (int)Math.Round(10000 * 12 * 0.8)
        },
        new SubscriptionPlan
        {
            Name = "Premium",
            Monthly = 13000,
            Quarterly = (int)Math.Round(13000 * 3 * 0.9),
            Yearly = (int)Math.Round(13000 * 12 * 0.8)
        }
    };

    public PaymentController(IRoleRepository roleRepository, IEmailService emailService, IInvoiceService invoiceService = null)
    {
        _roleRepository = roleRepository;
        _emailService = emailService;
        _invoiceService = invoiceService;
    }

    // ✅ Create Razorpay Order
    [HttpPost("create-order")]
    public IActionResult CreateOrder([FromBody] OrderRequest request)
    {
        RazorpayClient client = new RazorpayClient(key, secret);

        var options = new Dictionary<string, object>
        {
            { "amount", request.Amount * 100 }, // Razorpay expects amount in paise
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
        if (data == null ||
            string.IsNullOrEmpty(data.RazorpayOrderId) ||
            string.IsNullOrEmpty(data.RazorpayPaymentId) ||
            string.IsNullOrEmpty(data.RazorpaySignature))
        {
            return BadRequest(new { success = false, message = "Missing required fields" });
        }
       
        try
        {
            // 1️⃣ Verify Razorpay signature
            string generatedSignature = RazorpayUtils.CalculateSignature(
                $"{data.RazorpayOrderId}|{data.RazorpayPaymentId}", secret);

            if (generatedSignature != data.RazorpaySignature)
                return BadRequest(new { success = false, message = "Invalid payment signature" });

            // 2️⃣ Get role/user
            var role = await _roleRepository.GetRoleByIdAsync(data.RoleId);
            if (role == null)
                return BadRequest(new { success = false, message = "Role not found" });

            // 3️⃣ Update subscription details
            role.PaidDate = DateTime.UtcNow;
            role.AmountPaid = data.Amount;
            role.IsActive = true;

            // Determine plan & subscription period from AmountPaid
            SubscriptionPlan matchedPlan = null;
            string subscriptionPeriod = "Month";

            foreach (var p in subscriptionPlans)
            {
                if (p.Monthly == (int)data.Amount)
                {
                    matchedPlan = p;
                    subscriptionPeriod = "Month";
                    break;
                }
                else if (p.Quarterly == (int)data.Amount)
                {
                    matchedPlan = p;
                    subscriptionPeriod = "3 Months";
                    break;
                }
                else if (p.Yearly == (int)data.Amount)
                {
                    matchedPlan = p;
                    subscriptionPeriod = "Year";
                    break;
                }
            }

            // Fallback if no match
            matchedPlan ??= subscriptionPlans.First();

            role.PlanName = matchedPlan.Name;
            role.SubscriptionPeriod = subscriptionPeriod;

            // Extend ValidUntil based on subscriptionPeriod
            role.ValidUntil = subscriptionPeriod switch
            {
                "Month" => (role.ValidUntil != null && role.ValidUntil > DateTime.UtcNow)
                            ? role.ValidUntil.Value.ToUniversalTime().AddMonths(1)
                            : DateTime.UtcNow.AddMonths(1),
                "3 Months" => (role.ValidUntil != null && role.ValidUntil > DateTime.UtcNow)
                            ? role.ValidUntil.Value.ToUniversalTime().AddMonths(3)
                            : DateTime.UtcNow.AddMonths(3),
                "Year" => (role.ValidUntil != null && role.ValidUntil > DateTime.UtcNow)
                            ? role.ValidUntil.Value.ToUniversalTime().AddYears(1)
                            : DateTime.UtcNow.AddYears(1),
                _ => DateTime.UtcNow.AddMonths(1)
            };

            // Update privileges if provided
            if (data.Privileges != null && data.Privileges.Count > 0)
            {
                role.Privileges = data.Privileges;
            }

            await _roleRepository.UpdateRoleAsync(role);

            // 4️⃣ Generate invoice PDF (optional)
            byte[] invoicePdf = null;
            if (_invoiceService != null)
            {
                invoicePdf = await _invoiceService.GenerateInvoicePdfAsync(
                    role.UserName,
                    role.UserEmail,
                    role.GymName,
                    role.PlanName,
                    role.AmountPaid ?? 0,
                    role.PaidDate ?? DateTime.UtcNow,
                    role.SubscriptionPeriod
                );
            }

            // 5️⃣ Send email to user
            if (_emailService != null)
            {
                await _emailService.SendSubscriptionInvoiceAsync(
                    to: role.UserEmail,
                    userName: role.UserName,
                    gymName: role.GymName,
                    planName: role.PlanName,
                    subscriptionPeriod: role.SubscriptionPeriod,
                    amount: role.AmountPaid ?? 0,
                    paidDate: role.PaidDate ?? DateTime.UtcNow,
                    invoiceBytes: invoicePdf
                );

                // 6️⃣ Send notification to CEO
                var ceoBody = $@"
Hello CEO,

A new payment has been successfully completed.

Details:
- User: {role.UserName ?? "N/A"} ({role.UserEmail ?? "N/A"})
- Gym: {role.GymName ?? "N/A"}
- Plan: {role.PlanName}
- Subscription Period: {role.SubscriptionPeriod}
- Amount Paid: {role.AmountPaid}
- Paid Date (UTC): {role.PaidDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}

Regards,
Zyct Payment System
";

                await _emailService.SendEmailAsync(
                    to: "zyct.official@gmail.com",
                    subject: $"Payment Received from {role.UserName ?? "Unknown User"}",
                    body: ceoBody,
                    attachment: invoicePdf,
                    attachmentName: "Invoice.pdf"
                );
            }

            return Ok(new { success = true, role });
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

        using var hmacsha256 = new HMACSHA256(keyBytes);
        byte[] hashMessage = hmacsha256.ComputeHash(messageBytes);
        return BitConverter.ToString(hashMessage).Replace("-", "").ToLower();
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
    public List<string> Privileges { get; set; }
}

// ✅ SubscriptionPlan class
public class SubscriptionPlan
{
    public string Name { get; set; }
    public int Monthly { get; set; }
    public int Quarterly { get; set; }
    public int Yearly { get; set; }
}
