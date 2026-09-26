using CreditWorksVehicleManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditWorksVehicleManagement.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }

    public DbSet<Manufacturer> Manufacturers { get; set; }

    public DbSet<VehicleCategory> VehicleCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vehicle>()
            .Property(v => v.WeightKg)
            .HasPrecision(10, 2);

        modelBuilder.Entity<VehicleCategory>()
    .Property(c => c.MinWeightKg)
    .HasPrecision(10, 2);

        modelBuilder.Entity<VehicleCategory>()
            .Property(c => c.MaxWeightKg)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Manufacturer>().HasData(
       new Manufacturer { Id = 1, Name = "Mazda" },
       new Manufacturer { Id = 2, Name = "Mercedes" },
       new Manufacturer { Id = 3, Name = "Honda" },
       new Manufacturer { Id = 4, Name = "Ferrari" },
       new Manufacturer { Id = 5, Name = "Toyota" }
   );

        modelBuilder.Entity<VehicleCategory>().HasData(
            new VehicleCategory
            {
                Id = 1,
                Name = "Light",
                MinWeightKg = 0,
                MaxWeightKg = 500,
                Icon = "🚗"
            },
            new VehicleCategory
            {
                Id = 2,
                Name = "Medium",
                MinWeightKg = 500,
                MaxWeightKg = 2500,
                Icon = "🚚"
            },
            new VehicleCategory
            {
                Id = 3,
                Name = "Heavy",
                MinWeightKg = 2500,
                MaxWeightKg = null,
                Icon = "🚛"
            }
        );
    }
}

