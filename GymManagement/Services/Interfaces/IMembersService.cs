using GymManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Interfaces
{
    public interface IMembersService
    {
        Task<IEnumerable<Member>> GetAllMembersAsync();
        Task<Member> GetMemberByIdAsync(int id);
        Task AddMemberAsync(Member member);
        Task UpdateMemberAsync(Member member);
        Task DeleteMemberAsync(int id);

        // ✅ New method to get members by GymId and GymName
        Task<IEnumerable<Member>> GetMembersByGymAsync(int gymId, string gymName);
    }
}
