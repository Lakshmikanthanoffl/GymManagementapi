using GymManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int roleId);
        Task AddRoleAsync(Role role);
        Task UpdateRoleAsync(Role role);
        Task DeleteRoleAsync(int roleId);
        Task<Role?> GetRoleByEmailAndPasswordAsync(string email, string password);

        // ✅ New method for fetching by GymId & GymName
        Task<Role?> GetRoleByGymIdAndNameAsync(int gymId, string gymName);
    }
}
