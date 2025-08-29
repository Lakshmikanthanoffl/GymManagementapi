using GymManagement.Interfaces;
using GymManagement.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembersController : ControllerBase
    {
        private readonly IMembersService _membersService;

        public MembersController(IMembersService membersService)
        {
            _membersService = membersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var members = await _membersService.GetAllMembersAsync();
            return Ok(members);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var member = await _membersService.GetMemberByIdAsync(id);
            if (member == null) return NotFound();
            return Ok(member);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Member member)
        {
            await _membersService.AddMemberAsync(member);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Member member)
        {
            if (id != member.Id) return BadRequest();
            await _membersService.UpdateMemberAsync(member);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _membersService.DeleteMemberAsync(id);
            return NoContent();
        }

        // ✅ Get members by GymId and GymName
        [HttpGet("by-gym")]
        public async Task<IActionResult> GetByGym([FromQuery] int gymId, [FromQuery] string gymName)
        {
            var members = await _membersService.GetMembersByGymAsync(gymId, gymName);
            return Ok(members);
        }

        // ✅ Attendance Endpoints
        [HttpPost("{id}/attendance")]
        public async Task<IActionResult> MarkAttendance(int id, [FromBody] AttendanceRequest request)
        {
            await _membersService.MarkAttendanceAsync(id, request.Date);
            return Ok();
        }




        // Get attendance dates for a member
        [HttpGet("{id}/attendance")]
        public async Task<IActionResult> GetAttendance(int id)
        {
            var attendance = await _membersService.GetAttendanceAsync(id);
            return Ok(attendance);
        }
    }
}
