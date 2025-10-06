using GymManagement.Interfaces;
using GymManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagement.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IInvoiceService _invoiceService; // use interface
        private readonly IEmailService _emailService;     // use interface

        public RoleService(
            IRoleRepository roleRepository,
            IInvoiceService invoiceService,
            IEmailService emailService)
        {
            _roleRepository = roleRepository;
            _invoiceService = invoiceService;
            _emailService = emailService;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            var roles = (await _roleRepository.GetAllRolesAsync()).ToList();
            var today = DateTime.UtcNow;

            foreach (var role in roles)
            {
                if (role.ValidUntil.HasValue && role.ValidUntil.Value < today && role.IsActive)
                {
                    role.IsActive = false; // auto deactivate expired roles
                    await _roleRepository.UpdateRoleAsync(role);
                }
            }

            return roles;
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _roleRepository.GetRoleByIdAsync(roleId);
        }

        // Add role + generate invoice + send email
        public async Task AddRoleAsync(Role role)
        {
            await _roleRepository.AddRoleAsync(role);

            // Fire and forget (background task for PDF + Email)
            _ = Task.Run(async () =>
            {
                var invoicePdf = await _invoiceService.GenerateInvoicePdfAsync(
                    role.UserName,
                    role.UserEmail,
                    role.GymName,
                    role.PlanName,
                    role.AmountPaid ?? 0,
                    role.PaidDate ?? DateTime.Now,
                    role.SubscriptionPeriod
                );

                await _emailService.SendSubscriptionInvoiceAsync(
                    to: role.UserEmail,
                    userName: role.UserName,
                    gymName: role.GymName,
                    planName: role.PlanName,
                    subscriptionPeriod: role.SubscriptionPeriod,
                    amount: role.AmountPaid ?? 0,
                    paidDate: role.PaidDate ?? DateTime.Now,
                    invoiceBytes: invoicePdf
                );
            });
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

        public async Task<Role?> GetRoleByGymIdAndNameAsync(int gymId, string gymName)
        {
            return await _roleRepository.GetRoleByGymIdAndNameAsync(gymId, gymName);
        }

        public async Task<IEnumerable<Role>> GetRolesByEmailAsync(string email)
        {
            return await _roleRepository.GetRolesByEmailAsync(email);
        }
    }
}
