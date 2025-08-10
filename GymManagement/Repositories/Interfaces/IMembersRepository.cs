using GymManagement.Models;

namespace GymManagement.Interfaces
{
    public interface IMembersRepository
    {
        Task<IEnumerable<Member>> GetAllAsync();
        Task<Member> GetByIdAsync(int id);
        Task AddAsync(Member member);
        Task UpdateAsync(Member member);
        Task DeleteAsync(int id);
    }
}
