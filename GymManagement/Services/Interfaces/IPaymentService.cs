using GymManagement.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Services
{
    public interface IPaymentService
    {
        Task<Payment> SavePaymentAsync(PaymentRequest request, IFormFile screenshotFile);
        Task<List<Payment>> GetAllPaymentsAsync();
        Task<List<Payment>> GetPaymentsByGymAsync(int gymId, string gymName);

        // 🔹 Delete payment by Id
        Task<bool> DeletePaymentAsync(int paymentId);

        // 🔹 Update/Edit Payment
        Task<Payment> UpdatePaymentAsync(int paymentId, PaymentRequest request, IFormFile screenshotFile);
    }
}
