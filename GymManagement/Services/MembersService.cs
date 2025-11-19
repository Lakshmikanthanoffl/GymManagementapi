using GymManagement.Interfaces;
using GymManagement.Models;
using MimeKit;
using System.Collections.Generic;
using System.Net.Mail;
using MailKit.Net.Smtp;
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

        public async Task SendQrEmailAsync(
    string username,
    string gymName,
    string gymUserEmail,
    string toEmail,
    string qrUrl)
        {
            try
            {
                // 1️⃣ Download QR
                using var httpClient = new HttpClient();
                var qrBytes = await httpClient.GetByteArrayAsync(qrUrl);
                using var stream = new MemoryStream(qrBytes);

                // 2️⃣ Build Email
                var message = new MimeMessage();

                // Sender → MUST be Gmail App password email stored in Render env
                message.From.Add(new MailboxAddress($"{gymName} | Zyct Technologies",
                    Environment.GetEnvironmentVariable("EMAIL_USER")));

                // Receiver
                message.To.Add(new MailboxAddress(username, toEmail));
                message.Subject = $"Your Membership QR Code – {gymName}";

                var body = new BodyBuilder
                {
                    HtmlBody = $@"
            <div style='font-family: Arial; font-size:14px;'>
                <p>Dear <strong>{username}</strong>,</p>
                <p>Your membership QR code is attached.</p>
                <p>Regards,<br><strong>{gymName}</strong></p>
            </div>
            ",
                    TextBody = $"Dear {username}, your QR code is attached."
                };

                // 3️⃣ Attach QR file
                string fileName = $"{username}_{gymName}_QR.png".Replace(" ", "_");
                body.Attachments.Add(fileName, stream);
                message.Body = body.ToMessageBody();

                // 4️⃣ SMTP
                using var smtp = new MailKit.Net.Smtp.SmtpClient();

                // Render fix — ignore certificate issues
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Connect Gmail SMTP
                await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

                // Authenticate using Render environment variables
                await smtp.AuthenticateAsync(
                    Environment.GetEnvironmentVariable("EMAIL_USER"),
                    Environment.GetEnvironmentVariable("EMAIL_PASS")
                );

                // Send email
                await smtp.SendAsync(message);

                // Disconnect
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("EMAIL ERROR: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }






    }
}
