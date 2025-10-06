// IEmailService.cs
using System;
using System.Threading.Tasks;

namespace GymManagement.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, byte[] attachment = null, string attachmentName = null);

        // Add this
        Task SendSubscriptionInvoiceAsync(
            string to,
            string userName,
            string gymName,
            string planName,
            string subscriptionPeriod,
            decimal amount,
            DateTime paidDate,
            byte[] invoiceBytes
        );
    }
}
