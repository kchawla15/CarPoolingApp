using CreditWorksVehicleManagement.Data;
using CreditWorksVehicleManagement.Models;
using CreditWorksVehicleManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace CreditWorksVehicleManagement.Tests;

public class CategoryChangeTests
{
    private static (ApplicationDbContext Context, CategoryService Service)
        CreateService()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.VehicleCategories.AddRange(
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
            });

        context.Manufacturers.Add(
            new Manufacturer
            {
                Id = 1,
                Name = "Toyota"
            });

        context.SaveChanges();

        return (context, new CategoryService(context));
    }

    [Fact]
    public async Task UpdateCategory_ShouldMoveAdjacentBoundary_AndReclassifyWeight()
    {
        var (context, service) = CreateService();

        var initialCategory =
            await service.GetCategoryForWeightAsync(2200m);

        Assert.NotNull(initialCategory);
        Assert.Equal("Medium", initialCategory.Name);

        var medium = await context.VehicleCategories
            .SingleAsync(c => c.Name == "Medium");

        // Create a separate object representing the submitted edit.
        // Do not modify the tracked entity directly.
        var categoryUpdate = new VehicleCategory
        {
            Id = medium.Id,
            Name = medium.Name,
            MinWeightKg = medium.MinWeightKg,
            MaxWeightKg = 2000m,
            Icon = medium.Icon
        };

        var result = await service.UpdateCategoryAsync(categoryUpdate);

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        var updatedMedium = await context.VehicleCategories
            .SingleAsync(c => c.Name == "Medium");

        var updatedHeavy = await context.VehicleCategories
            .SingleAsync(c => c.Name == "Heavy");

        Assert.Equal(500m, updatedMedium.MinWeightKg);
        Assert.Equal(2000m, updatedMedium.MaxWeightKg);

        Assert.Equal(2000m, updatedHeavy.MinWeightKg);
        Assert.Null(updatedHeavy.MaxWeightKg);

        var updatedCategory =
            await service.GetCategoryForWeightAsync(2200m);

        Assert.NotNull(updatedCategory);
        Assert.Equal("Heavy", updatedCategory.Name);
    }

    [Fact]
    public async Task ExistingVehicleWeight_ShouldRemainUnchanged_WhenCategoryChanges()
    {
        var (context, service) = CreateService();

        var vehicle = new Vehicle
        {
            Id = 1,
            OwnerName = "Test Owner",
            ManufacturerId = 1,
            YearOfManufacture = 2020,
            WeightKg = 2200m
        };

        context.Vehicles.Add(vehicle);

        await context.SaveChangesAsync();

        var medium = await context.VehicleCategories
            .SingleAsync(c => c.Name == "Medium");

        // Create a separate object representing the submitted edit.
        var categoryUpdate = new VehicleCategory
        {
            Id = medium.Id,
            Name = medium.Name,
            MinWeightKg = medium.MinWeightKg,
            MaxWeightKg = 2000m,
            Icon = medium.Icon
        };

        var result = await service.UpdateCategoryAsync(categoryUpdate);

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);

        var storedVehicle = await context.Vehicles
            .SingleAsync(v => v.Id == vehicle.Id);

        // Category changes must not modify the vehicle's stored weight.
        Assert.Equal(2200m, storedVehicle.WeightKg);

        // The same vehicle should now be classified using
        // the updated category configuration.
        var category =
            await service.GetCategoryForWeightAsync(
                storedVehicle.WeightKg);

        Assert.NotNull(category);
        Assert.Equal("Heavy", category.Name);
    }
}