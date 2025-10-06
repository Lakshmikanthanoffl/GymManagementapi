using System;
using System.Threading.Tasks;

namespace GymManagement.Interfaces
{
    public interface IInvoiceService
    {
        Task<byte[]> GenerateInvoicePdfAsync(
            string userName,
            string email,
            string gymName,
            string planName,
            decimal amount,
            DateTime paidDate,
            string subscriptionPeriod
        );
    }
}
