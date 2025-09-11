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
        public async Task<IActionResult> CreateRole([FromBody] Role role)
        {
            if (role == null)
                return BadRequest(new { message = "Invalid role data" });

            role.IsActive = true;
            role.PaidDate ??= DateTime.Now;

            // ✅ Ensure privileges is not null
            role.Privileges ??= new List<string>();

            await _roleService.AddRoleAsync(role);
            return Ok(new { message = "Role created successfully" });
        }

        // PUT: api/role/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, [FromBody] Role role)
        {
            if (role == null || role.RoleId != id)
                return BadRequest(new { message = "Invalid role data" });

            var existingRole = await _roleService.GetRoleByIdAsync(id);
            if (existingRole == null)
                return NotFound(new { message = "Role not found" });

            existingRole.RoleName = role.RoleName;
            existingRole.UserName = role.UserName;
            existingRole.UserEmail = role.UserEmail;
            if (!string.IsNullOrEmpty(role.Password))
                existingRole.Password = role.Password; // ⚠️ hash in prod
            existingRole.GymId = role.GymId;
            existingRole.GymName = role.GymName;
            existingRole.PaidDate = role.PaidDate;
            existingRole.ValidUntil = role.ValidUntil;
            existingRole.AmountPaid = role.AmountPaid;
            existingRole.IsActive = role.IsActive;

            // ✅ Save privileges too
            existingRole.Privileges = role.Privileges ?? new List<string>();

            await _roleService.UpdateRoleAsync(existingRole);
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

        // POST: api/role/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Email and password are required" });

            var role = await _roleService.GetRoleByEmailAndPasswordAsync(request.Email, request.Password);
            if (role == null)
                return Unauthorized(new { message = "Invalid email or password" });

            if (role.ValidUntil.HasValue && role.ValidUntil.Value < DateTime.UtcNow)
            {
                role.IsActive = false;
                await _roleService.UpdateRoleAsync(role);
                return Unauthorized(new { message = "Account expired. Please renew subscription." });
            }

            return Ok(new
            {
                role.RoleId,
                role.RoleName,
                role.UserName,
                role.UserEmail,
                role.GymId,
                role.GymName,
                role.PaidDate,
                role.ValidUntil,
                role.AmountPaid,
                role.IsActive,
                role.Privileges // ✅ send menus
            });
        }


        // POST: api/role/bygym
        [HttpPost("bygym")]
        public async Task<IActionResult> FetchByGym([FromBody] gymidandname request)
        {
            if (request.GymId <= 0 || string.IsNullOrEmpty(request.GymName))
                return BadRequest(new { message = "GymId and GymName are required" });

            var role = await _roleService.GetRoleByGymIdAndNameAsync(request.GymId, request.GymName);
            if (role == null)
                return NotFound(new { message = "No role found for this gym" });

            return Ok(role);
        }
    }
}
