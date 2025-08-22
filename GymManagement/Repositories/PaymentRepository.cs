using GymManagement.Data;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Add Payment
        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        // Get all payments
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        // Get payments by Gym
        public async Task<List<Payment>> GetPaymentsByGymAsync(int gymId, string gymName)
        {
            return await _context.Payments
                                 .Where(p => p.GymId == gymId && p.GymName == gymName)
                                 .ToListAsync();
        }

        // Get payment by Id
        public async Task<Payment> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        // Delete Payment
        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
                return false;

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return true;
        }

        // 🔹 Edit/Update Payment
        public async Task<Payment> UpdatePaymentAsync(Payment updatedPayment)
        {
            var existingPayment = await _context.Payments.FindAsync(updatedPayment.PaymentId);
            if (existingPayment == null)
                return null; // payment not found

            // Update fields
            existingPayment.UserName = updatedPayment.UserName;
            existingPayment.Plan = updatedPayment.Plan;
            existingPayment.Price = updatedPayment.Price;
            existingPayment.PaymentDate = updatedPayment.PaymentDate;
            existingPayment.GymId = updatedPayment.GymId;
            existingPayment.GymName = updatedPayment.GymName;
            existingPayment.Screenshot = updatedPayment.Screenshot;

            _context.Payments.Update(existingPayment);
            await _context.SaveChangesAsync();

            return existingPayment;
        }
    }
}
