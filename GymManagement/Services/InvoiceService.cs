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

        
    }
}
