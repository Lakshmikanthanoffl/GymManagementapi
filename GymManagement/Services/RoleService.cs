using GymManagement.Interfaces;
using GymManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }


        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            var roles = (await _roleRepository.GetAllRolesAsync()).ToList();
            var today = DateTime.UtcNow;

            bool updated = false;

            foreach (var role in roles)
            {
                if (role.ValidUntil.HasValue && role.ValidUntil.Value < today && role.IsActive)
                {
                    role.IsActive = false; // auto deactivate expired roles
                    await _roleRepository.UpdateRoleAsync(role);
                    updated = true;
                }
            }

            return roles;
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _roleRepository.GetRoleByIdAsync(roleId);
        }

        public async Task AddRoleAsync(Role role)
        {
            await _roleRepository.AddRoleAsync(role);
        }

        public async Task UpdateRoleAsync(Role role)
        {
            await _roleRepository.UpdateRoleAsync(role);
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            await _roleRepository.DeleteRoleAsync(roleId);
        }

        public async Task<Role?> GetRoleByEmailAndPasswordAsync(string email, string password)
        {
            return await _roleRepository.GetRoleByEmailAndPasswordAsync(email, password);
        }

        // ✅ New method for fetching by GymId & GymName
        public async Task<Role?> GetRoleByGymIdAndNameAsync(int gymId, string gymName)
        {
            return await _roleRepository.GetRoleByGymIdAndNameAsync(gymId, gymName);
        }

        // ✅ New method: Fetch all roles by UserEmail
        public async Task<IEnumerable<Role>> GetRolesByEmailAsync(string email)
        {
            return await _roleRepository.GetRolesByEmailAsync(email);
        }
    }
}
