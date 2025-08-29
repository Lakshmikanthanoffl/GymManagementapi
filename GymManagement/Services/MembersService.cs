using GymManagement.Interfaces;
using GymManagement.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GymManagement.Services
{
    public class MembersService : IMembersService
    {
        private readonly IMembersRepository _repository;

        public MembersService(IMembersRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Member>> GetAllMembersAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Member> GetMemberByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task AddMemberAsync(Member member)
        {
            await _repository.AddAsync(member);
        }

        public async Task UpdateMemberAsync(Member member)
        {
            await _repository.UpdateAsync(member);
        }

        public async Task DeleteMemberAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        // ✅ Get members by Gym
        public async Task<IEnumerable<Member>> GetMembersByGymAsync(int gymId, string gymName)
        {
            return await _repository.GetMembersByGymAsync(gymId, gymName);
        }

        // ✅ Attendance methods

        public async Task MarkAttendanceAsync(int memberId, string date)
        {
            await _repository.MarkAttendanceAsync(memberId, date);
        }

        public async Task<IEnumerable<string>> GetAttendanceAsync(int memberId)
        {
            return await _repository.GetAttendanceAsync(memberId);
        }
    }
}
