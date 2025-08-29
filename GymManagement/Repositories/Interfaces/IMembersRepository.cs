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

        // ✅ New methods for Attendance
        Task MarkAttendanceAsync(int memberId, string date); // Add a date to attendance
        Task<IEnumerable<string>> GetAttendanceAsync(int memberId); // Get attendance dates
    }
}
