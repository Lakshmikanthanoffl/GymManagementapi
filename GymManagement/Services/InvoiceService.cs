using GymManagement.Interfaces;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GymManagement.Services
{
    public class InvoiceService : IInvoiceService
    {
        public async Task<byte[]> GenerateInvoicePdfAsync(
            string userName,
            string email,
            string gymName,
            string planName,
            decimal amount,
            DateTime paidDate,
            string subscriptionPeriod
        )
        {
            return await Task.Run(() =>
            {
                using var ms = new MemoryStream();
                var doc = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(doc, ms);
                doc.Open();

                // ===== Colors and Fonts =====
                var primaryColor = new BaseColor(33, 64, 154); // Navy blue
                var lightGray = new BaseColor(245, 245, 245);
                var boldFont = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.BLACK);
                var normalFont = FontFactory.GetFont("Arial", 11, Font.NORMAL, BaseColor.BLACK);
                var headerFont = FontFactory.GetFont("Arial", 22, Font.BOLD, primaryColor);

                // ===== "PAID" Watermark =====
                PdfContentByte canvas = writer.DirectContentUnder;
                Font watermarkFont = FontFactory.GetFont("Arial", 60, Font.BOLD, new GrayColor(0.90f));
                ColumnText.ShowTextAligned(
                    canvas, Element.ALIGN_CENTER,
                    new Phrase("PAID", watermarkFont),
                    300, 400, 45
                );

                // ===== Header =====
                Paragraph title = new Paragraph("INVOICE", headerFont)
                {
                    Alignment = Element.ALIGN_LEFT,
                    SpacingAfter = 10
                };
                doc.Add(title);

                // ===== Company Info =====
                PdfPTable companyTable = new PdfPTable(1);
                companyTable.WidthPercentage = 100;

                PdfPCell companyCell = new PdfPCell();
                companyCell.Border = Rectangle.NO_BORDER;
                companyCell.AddElement(new Paragraph("Zyct Technology", boldFont));
   
                companyCell.AddElement(new Paragraph("Email: zyct.official@gmail.com", normalFont));
                companyCell.AddElement(new Paragraph("Phone: 9025275948", normalFont));

                companyTable.AddCell(companyCell);
                doc.Add(companyTable);
                doc.Add(new Paragraph("\n"));

                // ===== Invoice Info =====
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 50f, 50f });

                PdfPCell leftCell = new PdfPCell();
                leftCell.Border = Rectangle.NO_BORDER;
                leftCell.AddElement(new Paragraph("BILL TO", boldFont));
                leftCell.AddElement(new Paragraph($"{userName}", normalFont));
                leftCell.AddElement(new Paragraph($"{email}", normalFont));

                PdfPCell rightCell = new PdfPCell();
                rightCell.Border = Rectangle.NO_BORDER;
                rightCell.AddElement(new Paragraph("INVOICE DETAILS", boldFont));
                rightCell.AddElement(new Paragraph($"Invoice #: ZY-{DateTime.Now:yyyyMMddHHmm}", normalFont));
                rightCell.AddElement(new Paragraph($"Invoice Date: {paidDate:dd/MM/yyyy}", normalFont));
                rightCell.AddElement(new Paragraph($"Status: PAID", boldFont));

                infoTable.AddCell(leftCell);
                infoTable.AddCell(rightCell);
                doc.Add(infoTable);

                doc.Add(new Paragraph("\n"));

                // ===== Items Table =====
                PdfPTable itemTable = new PdfPTable(4);
                itemTable.WidthPercentage = 100;
                itemTable.SetWidths(new float[] { 10f, 50f, 20f, 20f });

                string[] headers = { "QTY", "DESCRIPTION", "UNIT PRICE", "AMOUNT" };
                foreach (var h in headers)
                {
                    var headerCell = new PdfPCell(new Phrase(h, boldFont))
                    {
                        BackgroundColor = lightGray,
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 8
                    };
                    itemTable.AddCell(headerCell);
                }

                // Row: Plan Info
                itemTable.AddCell(new PdfPCell(new Phrase("1", normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER });
                itemTable.AddCell(new PdfPCell(new Phrase($"{planName} Plan ({subscriptionPeriod})", normalFont)));
                itemTable.AddCell(new PdfPCell(new Phrase($"₹{amount:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemTable.AddCell(new PdfPCell(new Phrase($"₹{amount:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                // Subtotal
                itemTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER, Colspan = 2 });
                itemTable.AddCell(new PdfPCell(new Phrase("Subtotal", boldFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemTable.AddCell(new PdfPCell(new Phrase($"₹{amount:F2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                // Total
                itemTable.AddCell(new PdfPCell(new Phrase("")) { Border = Rectangle.NO_BORDER, Colspan = 2 });
                itemTable.AddCell(new PdfPCell(new Phrase("TOTAL", boldFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                itemTable.AddCell(new PdfPCell(new Phrase($"₹{amount:F2}", boldFont)) { HorizontalAlignment = Element.ALIGN_RIGHT });

                doc.Add(itemTable);

                doc.Add(new Paragraph("\n\n"));

                // ===== Footer =====
                var thankYou = new Paragraph("Thank you for your payment!", boldFont)
                {
                    Alignment = Element.ALIGN_CENTER,
                    SpacingAfter = 10
                };
                doc.Add(thankYou);

                doc.Add(new Paragraph("Payment has been received successfully. No further action is required.", normalFont));
                doc.Add(new Paragraph("For any queries, contact: zyct.official@gmail.com | +91 9025275948", normalFont));

                doc.Close();
                return ms.ToArray();
            });
        }

        // ===== SEND EMAIL METHOD =====
        public async Task SendInvoiceEmailAsync(
            string userName,
            string email,
            string gymName,
            string planName,
            decimal amount,
            DateTime paidDate,
            string subscriptionPeriod,
            byte[] invoiceBytes
        )
        {
            string emailTemplate = @"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <title>Zyct Technology Invoice Confirmation</title>
  <style>
    body { font-family: Arial, sans-serif; background-color: #f6f8fb; margin: 0; padding: 30px; }
    .container { max-width: 600px; margin: auto; background: #fff; border-radius: 10px; overflow: hidden; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
    .header { background: #21409A; color: #fff; padding: 20px; text-align: center; }
    .header a { color: #fff; text-decoration: none; }
    .content { padding: 30px; color: #333; }
    .content h3 { color: #21409A; margin-top: 0; }
    .content table { width: 100%; border-collapse: collapse; margin-top: 20px; }
    .content td { padding: 10px; font-size: 15px; }
    .content tr:nth-child(even) { background: #f1f1f1; }
    .footer { background: #f1f1f1; text-align: center; padding: 15px; font-size: 13px; color: #555; }
    .btn { background: #21409A; color: #fff; padding: 10px 25px; border-radius: 5px; text-decoration: none; font-weight: bold; display: inline-block; margin-top: 20px; }
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h2>Zyct Technology</h2>
      <p>Email: <a href='mailto:zyvt.official@gmail.com'>zyct.official@gmail.com</a> | Phone: 9025275948</p>
    </div>

    <div class='content'>
      <h3>Hello {userName},</h3>
      <p>Your subscription has been successfully created and your payment has been received.</p>

      <table>
        <tr><td><b>Gym</b></td><td>{gymName}</td></tr>
        <tr><td><b>Plan</b></td><td>{planName}</td></tr>
        <tr><td><b>Subscription Period</b></td><td>{subscriptionPeriod}</td></tr>
        <tr><td><b>Paid Amount</b></td><td>₹{amount}</td></tr>
        <tr><td><b>Paid Date</b></td><td>{paidDate}</td></tr>
        <tr><td style='color:green;'><b>Payment Status</b></td><td style='color:green;'><b>PAID</b></td></tr>
      </table>

      <p>Please find your official invoice attached to this email.</p>
      <p>Thank you for choosing <b>Zyct Technology Pvt. Ltd.</b>!</p>

      <div style='text-align:center'>
        <a class='btn' href='#'>View Subscription</a>
      </div>
    </div>

    <div class='footer'>
      <p>Zyct Technology | Chennai, Tamil Nadu</p>
      <p>© {year} All rights reserved.</p>
    </div>
  </div>
</body>
</html>";

            // Replace placeholders
            string htmlBody = emailTemplate
                .Replace("{userName}", userName)
                .Replace("{gymName}", gymName)
                .Replace("{planName}", planName)
                .Replace("{subscriptionPeriod}", subscriptionPeriod)
                .Replace("{amount}", amount.ToString("F2"))
                .Replace("{paidDate}", paidDate.ToString("dd-MM-yyyy"))
                .Replace("{year}", DateTime.Now.Year.ToString());

            // Prepare email
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("zyct.official@gmail.com", "Zyct Fitness Pvt. Ltd.");
            mail.To.Add(email);
            mail.Subject = "Your Zyct Fitness Invoice";
            mail.Body = htmlBody;
            mail.IsBodyHtml = true;

            // Attach invoice PDF
            mail.Attachments.Add(new Attachment(new MemoryStream(invoiceBytes), "Invoice.pdf"));

            // Send using Gmail SMTP
            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("zyct.official@gmail.com", "sprgmexakzwwzuho");
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(mail);
            }
        }
    }
}
