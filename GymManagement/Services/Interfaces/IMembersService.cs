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

        // ✅ Get members by GymId and GymName
        Task<IEnumerable<Member>> GetMembersByGymAsync(int gymId, string gymName);

        // ✅ Attendance methods
        Task MarkAttendanceAsync(int memberId, string date);       // Mark a date for a member
        Task<IEnumerable<string>> GetAttendanceAsync(int memberId); // Get attendance dates for a member

        Task SendQrEmailAsync(string username, string gymName, string gymUserEmail, string toEmail, string qrUrl);



    }
}
