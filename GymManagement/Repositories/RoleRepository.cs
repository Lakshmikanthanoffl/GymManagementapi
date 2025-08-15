using GymManagement.Data;
using GymManagement.Interfaces;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles.FindAsync(roleId);
        }

        public async Task AddRoleAsync(Role role)
        {
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRoleAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRoleAsync(int roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Role?> GetRoleByEmailAndPasswordAsync(string email, string password)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.UserEmail == email && r.Password == password);
        }
        public async Task<Role?> GetRoleByGymIdAndNameAsync(int gymId, string gymName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.GymId == gymId && r.GymName == gymName);
        }

    }
}
