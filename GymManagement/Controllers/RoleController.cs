using GymManagement.Interfaces;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/role
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        // GET: api/role/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
                return NotFound(new { message = "Role not found" });

            return Ok(role);
        }

        // POST: api/role
        [HttpPost]
        public async Task<IActionResult> CreateRole(Role role)
        {
            if (role == null)
                return BadRequest(new { message = "Invalid role data" });

            await _roleService.AddRoleAsync(role);
            return Ok(new { message = "Role created successfully" });
        }

        // PUT: api/role/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, Role role)
        {
            if (role == null || role.RoleId != id) // ✅ fixed: RoleId instead of Id
                return BadRequest(new { message = "Invalid role data" });

            var existingRole = await _roleService.GetRoleByIdAsync(id);
            if (existingRole == null)
                return NotFound(new { message = "Role not found" });

            await _roleService.UpdateRoleAsync(role);
            return Ok(new { message = "Role updated successfully" });
        }

        // DELETE: api/role/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var existingRole = await _roleService.GetRoleByIdAsync(id);
            if (existingRole == null)
                return NotFound(new { message = "Role not found" });

            await _roleService.DeleteRoleAsync(id);
            return Ok(new { message = "Role deleted successfully" });
        }
        // GET: api/role/login?email=...&password=...
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var role = await _roleService.GetRoleByEmailAndPasswordAsync(request.Email, request.Password);
            if (role == null)
                return Unauthorized(new { message = "Invalid email or password" });

            // Only return safe fields, not the password
            return Ok(new
            {
                role.RoleId,
                role.RoleName,
                role.UserName,
                role.UserEmail
            });
        }

    }
}
