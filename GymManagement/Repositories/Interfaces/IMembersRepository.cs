using GymManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Interfaces
{
    public interface IMembersRepository
    {
        Task<IEnumerable<Member>> GetAllAsync();
        Task<Member> GetByIdAsync(int id);
        Task AddAsync(Member member);
        Task UpdateAsync(Member member);
        Task DeleteAsync(int id);

        // ✅ New method to get members by GymId or GymName
        Task<IEnumerable<Member>> GetMembersByGymAsync(int gymId, string gymName);
    }
}
