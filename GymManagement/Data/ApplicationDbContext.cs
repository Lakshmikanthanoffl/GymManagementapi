using Microsoft.EntityFrameworkCore;
using GymManagement.Models;

namespace GymManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Member> Members { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Set SubscriptionType to be stored as JSON
            modelBuilder.Entity<Member>()
                .Property(m => m.SubscriptionType)
                .HasColumnType("jsonb"); // or "json" for MySQL or older PostgreSQL

            base.OnModelCreating(modelBuilder);
        }
    }
}
