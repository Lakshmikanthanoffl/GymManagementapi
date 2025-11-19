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

        public async Task SendQrEmailAsync(string username, string gymName, string gymUserEmail, string toEmail, string qrUrl)
        {
            using var httpClient = new HttpClient();

            // Download QR image bytes
            var qrBytes = await httpClient.GetByteArrayAsync(qrUrl);

            // Create MemoryStream safely
            var stream = new MemoryStream(qrBytes);
            stream.Position = 0;

            var message = new MimeMessage();

            // Brevo requires FROM email to be a verified sender
            message.From.Add(new MailboxAddress($"{gymName} | Zyct", "9bf73e001@smtp-brevo.com"));

            // Gym original email (reply-to)
            message.ReplyTo.Add(new MailboxAddress(gymName, gymUserEmail));

            message.To.Add(new MailboxAddress(username, toEmail));
            message.Subject = $"Your Membership QR Code – {gymName}";

            var body = new BodyBuilder
            {
                HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; font-size:14px; color:#333;'>
                <p>Dear <strong>{username}</strong>,</p>

                <p>Thank you for being a valued member of <strong>{gymName}</strong>.</p>

                <p>Your membership QR code is attached to this email.</p>

                <br>

                <p>Regards,<br>
                <strong>{gymName}</strong></p>

                <hr style='margin-top:25px; border:none; border-top:1px solid #ddd;' />

                <p style='font-size:12px; color:#666;'>
                    This email was sent securely via <strong>Zyct Technologies</strong>.
                </p>
            </div>",
                TextBody = $@"
Dear {username},

Your membership QR code is attached.
Thank you for being a part of {gymName}.

Regards,
{gymName}

---
Powered by Zyct Technologies"
            };

            // File name (safe)
            string fileName = $"{username}_{gymName}_QR.png".Replace(" ", "_");

            // Attach QR image
            body.Attachments.Add(fileName, stream);

            message.Body = body.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            await smtp.ConnectAsync("smtp-relay.brevo.com", 587, MailKit.Security.SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync("9bf73e001@smtp-brevo.com", "RWrKJ15yvbafhFVN");

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }





    }
}
