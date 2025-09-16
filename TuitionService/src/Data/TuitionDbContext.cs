using Microsoft.EntityFrameworkCore;
using src.Entities;

namespace src.Data
{
    public class TuitionDbContext : DbContext
    {
        public TuitionDbContext(DbContextOptions<TuitionDbContext> options) : base(options)
        {
        }

        public DbSet<Tuition> Tuitions { get; set; }
    }
}
