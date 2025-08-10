using GymManagement.Models;

namespace GymManagement.Interfaces
{
    public interface IMembersService
    {
        Task<IEnumerable<Member>> GetAllMembersAsync();
        Task<Member> GetMemberByIdAsync(int id);
        Task AddMemberAsync(Member member);
        Task UpdateMemberAsync(Member member);
        Task DeleteMemberAsync(int id);
    }
}
