using GymManagement.Models;
using GymManagement.Repositories;
using Microsoft.AspNetCore.Http;
using Supabase;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

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

        public async Task<Payment> SavePaymentAsync(PaymentRequest request, IFormFile screenshotFile)
        {
            string screenshotUrl = null;

            if (screenshotFile != null)
            {
                var bucket = _supabaseClient.Storage.From("member-photos");

                // Save IFormFile to temp file
                var tempPath = Path.Combine(Path.GetTempPath(), screenshotFile.FileName);
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await screenshotFile.CopyToAsync(stream);
                }

                // Upload file
                await bucket.Upload(tempPath, screenshotFile.FileName);

                
                int fiftyYearsInSeconds = 10 * 365 * 24 * 60 * 60;

                screenshotUrl = await bucket.CreateSignedUrl(screenshotFile.FileName, fiftyYearsInSeconds);

                // Delete temp file
                File.Delete(tempPath);
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



        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _repository.GetAllPaymentsAsync();
        }

        public async Task<List<Payment>> GetPaymentsByGymAsync(int gymId, string gymName)
        {
            return await _repository.GetPaymentsByGymAsync(gymId, gymName);
        }

    }
}
