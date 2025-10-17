using Microsoft.EntityFrameworkCore;
using src.Entities;

namespace src.Data
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
        {
        }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>()
                .HasIndex(p => p.TuitionId)
                .IsUnique()
                .HasFilter("\"Status\" = 'success'");
        }
    }
}
