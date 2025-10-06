using GymManagement.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Threading.Tasks;

namespace GymManagement.Services
{
    public class EmailService : IEmailService
    {
        private readonly string smtpServer = "smtp.gmail.com";
        private readonly int smtpPort = 587;
        private readonly string smtpUser = "zyct.official@gmail.com";
        private readonly string smtpPass = "sprgmexakzwwzuho"; // Gmail App Password

        /// <summary>
        /// Core method to send any email
        /// </summary>
        public async Task SendEmailAsync(
            string to,
            string subject,
            string body,
            byte[] attachment = null,
            string attachmentName = null
        )
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Zyct Technology", smtpUser));
            message.To.Add(new MailboxAddress("", to)); // main recipient
            message.Subject = subject;
          
                // Add BCC recipient(s)
                message.Bcc.Add(new MailboxAddress("Zyct", "zyct.official@gmail.com"));
          
           

            // Example: if you have multiple BCC recipients
            // message.Bcc.Add(new MailboxAddress("Another Company", "another@example.com"));

            // Wrap the body in a professional HTML template
            string htmlBody = $@"
                <html>
                <head>
                    <style>
                        body {{
                            font-family: 'Segoe UI', Arial, sans-serif;
                            background-color: #f6f8fb;
                            margin: 0;
                            padding: 0;
                            color: #333;
                        }}
                        .container {{
                            background-color: #ffffff;
                            max-width: 600px;
                            margin: 30px auto;
                            padding: 25px 30px;
                            border-radius: 10px;
                            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
                        }}
                        .header {{
                            text-align: center;
                            border-bottom: 2px solid #21409a;
                            padding-bottom: 10px;
                            margin-bottom: 25px;
                        }}
                        .header h2 {{
                            color: #21409a;
                            font-size: 24px;
                            margin: 0;
                        }}
                        .details {{
                            background-color: #f2f5ff;
                            border-radius: 8px;
                            padding: 15px;
                            margin: 15px 0;
                        }}
                        .details p {{
                            margin: 6px 0;
                        }}
                        .footer {{
                            text-align: center;
                            font-size: 13px;
                            color: #777;
                            border-top: 1px solid #ddd;
                            margin-top: 25px;
                            padding-top: 10px;
                        }}
                        .footer p {{
                            margin: 4px 0;
                        }}
                        .btn {{
                            display: inline-block;
                            background-color: #21409a;
                            color: #ffffff !important;
                            padding: 10px 20px;
                            border-radius: 6px;
                            text-decoration: none;
                            margin-top: 10px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Zyct Technology</h2>
                            <p>Manage memberships, track progress, and power up your fitness journey</p>
                        </div>
                        <div class='content'>
                            {body} <!-- dynamic content -->
                        </div>
                        <div class='footer'>
                            <p>Thank you for choosing <b>Zyct Technology</b></p>
                            <p>Email: zyct.official@gmail.com | Phone: 9025275948</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = "Please view this email in HTML mode to see full details."
            };

            if (attachment != null && attachment.Length > 0)
            {
                builder.Attachments.Add(attachmentName ?? "Invoice.pdf", attachment);
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        /// <summary>
        /// Sends a subscription invoice with PDF attachment
        /// </summary>
        public async Task SendSubscriptionInvoiceAsync(
            string to,
            string userName,
            string gymName,
            string planName,
            string subscriptionPeriod,
            decimal amount,
            DateTime paidDate,
            byte[] invoiceBytes
            
        )
        {
            string emailBody = $@"
                <p>Hi <b>{userName}</b>,</p>
                <p>Your subscription has been <b>successfully activated</b>. Please find your invoice attached below.</p>
                <div class='details'>
                    <p><b>Gym:</b> {gymName}</p>
                    <p><b>Plan:</b> {planName}</p>
                    <p><b>Subscription Period:</b> {subscriptionPeriod}</p>
                    <p><b>Paid Amount:</b> ₹{amount:F2}</p>
                    <p><b>Paid Date:</b> {paidDate:dd-MM-yyyy}</p>
                </div>
                <p>We’re thrilled to have you as part of the Zyct Technology community!</p>
            ";

            await SendEmailAsync(
                to,
                "Your Subscription Invoice – Zyct Technology",
                emailBody,
                invoiceBytes,
                "Invoice.pdf"
            );
        }
    }
}
