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

        // Tables
        public DbSet<Member> Members { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Store SubscriptionType as JSON in PostgreSQL
            modelBuilder.Entity<Member>()
                .Property(m => m.SubscriptionType)
                .HasColumnType("jsonb");

            base.OnModelCreating(modelBuilder);
        }
    }
}
