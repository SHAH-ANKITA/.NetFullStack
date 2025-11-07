using GenerationPlanMLM.Models;
using Microsoft.EntityFrameworkCore;

namespace GenerationPlanMLM.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<IncomeRecord> IncomeRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Sponsor)
                .WithMany(u => u.Referrals)
                .HasForeignKey(u => u.SponsorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserId)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.MobileNumber)
                .IsUnique();
        }
    }
}

