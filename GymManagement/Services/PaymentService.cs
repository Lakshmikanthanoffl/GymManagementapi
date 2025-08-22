using GymManagement.Models;
using GymManagement.Repositories;
using Microsoft.AspNetCore.Http;
using Supabase;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System;

namespace GymManagement.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly Client _supabaseClient;

        public PaymentService(IPaymentRepository repository, Client supabaseClient)
        {
            _repository = repository;
            _supabaseClient = supabaseClient;
        }

        // Save new payment
        public async Task<Payment> SavePaymentAsync(PaymentRequest request, IFormFile screenshotFile)
        {
            string screenshotUrl = null;

            if (screenshotFile != null)
            {
                screenshotUrl = await UploadScreenshot(screenshotFile);
            }

            var payment = new Payment
            {
                UserName = request.UserName,
                Plan = request.Plan,
                Price = request.Price,
                PaymentDate = request.PaymentDate,
                GymId = request.GymId,
                GymName = request.GymName,
                Screenshot = screenshotUrl
            };

            return await _repository.AddPaymentAsync(payment);
        }

        // Update/Edit payment
        public async Task<Payment> UpdatePaymentAsync(int paymentId, PaymentRequest request, IFormFile screenshotFile)
        {
            var existingPayment = await _repository.GetPaymentByIdAsync(paymentId);
            if (existingPayment == null)
                return null;

            string screenshotUrl = existingPayment.Screenshot;

            // If a new file is provided, replace the old one
            if (screenshotFile != null)
            {
                // Delete old screenshot if exists
                if (!string.IsNullOrEmpty(existingPayment.Screenshot))
                {
                    try
                    {
                        var bucket = _supabaseClient.Storage.From("member-photos");
                        var uri = new Uri(existingPayment.Screenshot);
                        var oldFileName = Path.GetFileName(uri.LocalPath);
                        await bucket.Remove(new List<string> { oldFileName });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old screenshot: {ex.Message}");
                    }
                }

                screenshotUrl = await UploadScreenshot(screenshotFile);
            }

            // Update fields
            existingPayment.UserName = request.UserName;
            existingPayment.Plan = request.Plan;
            existingPayment.Price = request.Price;
            existingPayment.PaymentDate = request.PaymentDate;
            existingPayment.GymId = request.GymId;
            existingPayment.GymName = request.GymName;
            existingPayment.Screenshot = screenshotUrl;

            return await _repository.UpdatePaymentAsync(existingPayment);
        }

        // Helper: Upload screenshot to Supabase
        private async Task<string> UploadScreenshot(IFormFile screenshotFile)
        {
            var bucket = _supabaseClient.Storage.From("member-photos");

            var tempPath = Path.Combine(Path.GetTempPath(), screenshotFile.FileName);
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                await screenshotFile.CopyToAsync(stream);
            }

            await bucket.Upload(tempPath, screenshotFile.FileName);

            int fiftyYearsInSeconds = 10 * 365 * 24 * 60 * 60;
            var url = await bucket.CreateSignedUrl(screenshotFile.FileName, fiftyYearsInSeconds);

            File.Delete(tempPath);
            return url;
        }

        // Get all payments
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _repository.GetAllPaymentsAsync();
        }

        // Get payments by gym
        public async Task<List<Payment>> GetPaymentsByGymAsync(int gymId, string gymName)
        {
            return await _repository.GetPaymentsByGymAsync(gymId, gymName);
        }

        // Delete payment
        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            var payment = await _repository.GetPaymentByIdAsync(paymentId);
            if (payment == null)
                return false;

            // Delete screenshot if exists
            if (!string.IsNullOrEmpty(payment.Screenshot))
            {
                try
                {
                    var bucket = _supabaseClient.Storage.From("member-photos");
                    var uri = new Uri(payment.Screenshot);
                    var fileName = Path.GetFileName(uri.LocalPath);
                    await bucket.Remove(new List<string> { fileName });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting screenshot: {ex.Message}");
                }
            }

            return await _repository.DeletePaymentAsync(paymentId);
        }
    }
}
