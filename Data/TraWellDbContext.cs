using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TraWell.Models;

namespace TraWell.Data
{
    public class TraWellDbContext : IdentityDbContext<ApplicationUser>
    {
        public TraWellDbContext(DbContextOptions<TraWellDbContext> options) : base(options) { }

        public DbSet<Place> Places { get; set; }
        public DbSet<TourPackage> TourPackages { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<PackageProvider> PackageProviders { get; set; }
        public DbSet<Video> Videos { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<TourPackage>()
                .HasOne(tp => tp.Provider)
                .WithMany(p => p.TourPackages)
                .HasForeignKey(tp => tp.ProviderId);

            modelBuilder.Entity<TourPackage>()
                .HasOne(tp => tp.Place)
                .WithMany(p => p.TourPackages)
                .HasForeignKey(tp => tp.PlaceId);

            modelBuilder.Entity<Hotel>()
                .HasOne(h => h.Place)
                .WithMany(p => p.Hotels)
                .HasForeignKey(h => h.PlaceId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.TourPackage)
                .WithMany(tp => tp.Bookings)
                .HasForeignKey(b => b.TourPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Hotel)
                .WithMany(h => h.Bookings)
                .HasForeignKey(b => b.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.TourPackage)
                .WithMany(tp => tp.Reviews)
                .HasForeignKey(r => r.TourPackageId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Hotel)
                .WithMany(h => h.Reviews)
                .HasForeignKey(r => r.HotelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserPreferences>()
                .HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId);

            // Configure decimal precision
            modelBuilder.Entity<TourPackage>()
                .Property(tp => tp.Price)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Hotel>()
                .Property(h => h.PricePerNight)
                .HasColumnType("decimal(8,2)");

            modelBuilder.Entity<Booking>()
                .Property(b => b.TotalAmount)
                .HasColumnType("decimal(12,2)");

            modelBuilder.Entity<PackageProvider>()
                .Property(pp => pp.CommissionRate)
                .HasColumnType("decimal(5,4)");

            // Configure indexes
            modelBuilder.Entity<Place>().HasIndex(p => p.Country);
            modelBuilder.Entity<TourPackage>().HasIndex(tp => tp.Category);
            modelBuilder.Entity<Hotel>().HasIndex(h => h.StarRating);

            // Configure JSON columns for lists with value comparers
            modelBuilder.Entity<Place>()
                .Property(p => p.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<string>() : c.ToList()));

            modelBuilder.Entity<Place>()
                .Property(p => p.ImageUrls)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<string>() : c.ToList()));

            modelBuilder.Entity<Place>()
                .Property(p => p.Categories)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<string>() : c.ToList()));

            modelBuilder.Entity<Place>()
                .Property(p => p.BestSeasons)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c == null ? new List<string>() : c.ToList()));

            // Configure other JSON list properties similarly for other entities
        }
    }
}
