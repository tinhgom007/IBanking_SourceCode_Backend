using Microsoft.EntityFrameworkCore;
using src.Entities;

namespace src.Data
{
    public class ProfileDbContext : DbContext
    {
        public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options) 
        { 
        }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentId)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.UserId)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.PhoneNumber)
                .IsUnique();
        }
    }
}


