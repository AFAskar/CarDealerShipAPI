using CarDealership.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Data;

public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships if needed
        builder.Entity<Sale>()
            .HasOne(s => s.User)
            .WithMany(u => u.Sales)
            .HasForeignKey(s => s.UserId);

        builder.Entity<Sale>()
            .HasOne(s => s.Vehicle)
            .WithMany()
            .HasForeignKey(s => s.VehicleId);

        builder.Entity<OtpCode>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId);
    }
}
