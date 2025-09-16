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
    }
}
