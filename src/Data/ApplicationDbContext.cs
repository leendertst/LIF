using Lif.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Lif.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Producten { get; set; } = null!;
        public DbSet<Klantlog> Klantlogs { get; set; } = null!;
        public DbSet<KlantlogProduct> KlantlogProducten { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Klantlog configuratie
            modelBuilder.Entity<Klantlog>()
                .HasOne(k => k.User)
                .WithMany()
                .HasForeignKey(k => k.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Klantlog>()
                .HasIndex(k => new { k.ApplicationUserId, k.WeekNummer, k.Jaar })
                .IsUnique();

            // KlantlogProduct configuratie
            modelBuilder.Entity<KlantlogProduct>()
                .HasOne(kp => kp.Klantlog)
                .WithMany(k => k.KlantlogProducten)
                .HasForeignKey(kp => kp.KlantlogId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KlantlogProduct>()
                .HasOne(kp => kp.Product)
                .WithMany()
                .HasForeignKey(kp => kp.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
