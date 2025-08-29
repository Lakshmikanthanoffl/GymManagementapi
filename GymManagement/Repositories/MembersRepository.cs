using GymManagement.Data;
using GymManagement.Interfaces;
using GymManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Repositories
{
    public class MembersRepository : IMembersRepository
    {
        private readonly ApplicationDbContext _context;

        public MembersRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Member>> GetAllAsync()
        {
            return await _context.Members.ToListAsync();
        }

        public async Task<Member> GetByIdAsync(int id)
        {
            return await _context.Members.FindAsync(id);
        }

        public async Task AddAsync(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Member member)
        {
            _context.Members.Update(member);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member != null)
            {
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Get members by GymId and GymName
        public async Task<IEnumerable<Member>> GetMembersByGymAsync(int gymId, string gymName)
        {
            return await _context.Members
                .Where(m => m.GymId == gymId && m.GymName == gymName)
                .ToListAsync();
        }

        // ✅ Mark attendance for a member
        public async Task MarkAttendanceAsync(int memberId, string date)
        {
            var member = await _context.Members.FindAsync(memberId);
            if (member != null)
            {
                if (member.Attendance == null)
                    member.Attendance = new List<string>();

                if (!member.Attendance.Contains(date))
                    member.Attendance.Add(date);

                _context.Members.Update(member);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Get attendance dates for a member
        public async Task<IEnumerable<string>> GetAttendanceAsync(int memberId)
        {
            var member = await _context.Members.FindAsync(memberId);
            return member?.Attendance ?? new List<string>();
        }
    }
}
