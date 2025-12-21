using Microsoft.EntityFrameworkCore;
using PremiumPlace_API.Models;

namespace PremiumPlace_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    
        public DbSet<Place> Places { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Place>()
                .Property(p => p.Rate)
                .HasPrecision(10, 2);

            modelBuilder.Entity<Place>()
                .Property(p => p.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
