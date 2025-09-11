using Microsoft.EntityFrameworkCore;
using GymManagement.Models;

namespace GymManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        public DbSet<Payment> Payments { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ Member: Map SubscriptionType to PostgreSQL jsonb
            modelBuilder.Entity<Member>()
                .Property(m => m.SubscriptionType)
                .HasColumnType("jsonb");

            // ✅ Role: Map Privileges to PostgreSQL text[]
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(r => r.RoleId);

                entity.Property(r => r.Privileges)
                      .HasColumnType("text[]")
                      .HasDefaultValueSql("'{}'::text[]"); // ✅ default empty array
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
