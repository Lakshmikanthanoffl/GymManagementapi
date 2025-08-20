using GymManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> AddPaymentAsync(Payment payment);
        Task<List<Payment>> GetAllPaymentsAsync();

        Task<List<Payment>> GetPaymentsByGymAsync(int gymId, string gymName);

    }
}
