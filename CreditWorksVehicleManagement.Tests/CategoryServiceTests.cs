using CreditWorksVehicleManagement.Data;
using CreditWorksVehicleManagement.Models;
using CreditWorksVehicleManagement.Services;
using Microsoft.EntityFrameworkCore;

namespace CreditWorksVehicleManagement.Tests;

public class CategoryServiceTests
{
    private static CategoryService CreateService()
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

        context.SaveChanges();

        return new CategoryService(context);
    }

    [Fact]
    public async Task Weight_499_99_Should_Be_Light()
    {
        var service = CreateService();

        var category = await service.GetCategoryForWeightAsync(499.99m);

        Assert.NotNull(category);
        Assert.Equal("Light", category.Name);
    }

    [Fact]
    public async Task Weight_500_Should_Be_Medium()
    {
        var service = CreateService();

        var category = await service.GetCategoryForWeightAsync(500.00m);

        Assert.NotNull(category);
        Assert.Equal("Medium", category.Name);
    }

    [Fact]
    public async Task Weight_2499_99_Should_Be_Medium()
    {
        var service = CreateService();

        var category = await service.GetCategoryForWeightAsync(2499.99m);

        Assert.NotNull(category);
        Assert.Equal("Medium", category.Name);
    }

    [Fact]
    public async Task Weight_2500_Should_Be_Heavy()
    {
        var service = CreateService();

        var category = await service.GetCategoryForWeightAsync(2500.00m);

        Assert.NotNull(category);
        Assert.Equal("Heavy", category.Name);
    }

    [Fact]
    public void CategoryConfiguration_WithGap_ShouldBeInvalid()
    {
        var service = CreateService();

        var categories = new List<VehicleCategory>
    {
        new()
        {
            Name = "Light",
            MinWeightKg = 0,
            MaxWeightKg = 500,
            Icon = "🚗"
        },
        new()
        {
            Name = "Medium",
            MinWeightKg = 600,
            MaxWeightKg = 2500,
            Icon = "🚚"
        },
        new()
        {
            Name = "Heavy",
            MinWeightKg = 2500,
            MaxWeightKg = null,
            Icon = "🚛"
        }
    };

        var result = service.ValidateConfiguration(categories);

        Assert.False(result.IsValid);
    }
    [Fact]
    public void CategoryConfiguration_WithOverlap_ShouldBeInvalid()
    {
        var service = CreateService();

        var categories = new List<VehicleCategory>
    {
        new()
        {
            Name = "Light",
            MinWeightKg = 0,
            MaxWeightKg = 600,
            Icon = "🚗"
        },
        new()
        {
            Name = "Medium",
            MinWeightKg = 500,
            MaxWeightKg = 2500,
            Icon = "🚚"
        },
        new()
        {
            Name = "Heavy",
            MinWeightKg = 2500,
            MaxWeightKg = null,
            Icon = "🚛"
        }
    };

        var result = service.ValidateConfiguration(categories);

        Assert.False(result.IsValid);
    }
    [Fact]
    public void CategoryConfiguration_NotStartingAtZero_ShouldBeInvalid()
    {
        var service = CreateService();

        var categories = new List<VehicleCategory>
    {
        new()
        {
            Name = "Light",
            MinWeightKg = 100,
            MaxWeightKg = 500,
            Icon = "🚗"
        },
        new()
        {
            Name = "Medium",
            MinWeightKg = 500,
            MaxWeightKg = 2500,
            Icon = "🚚"
        },
        new()
        {
            Name = "Heavy",
            MinWeightKg = 2500,
            MaxWeightKg = null,
            Icon = "🚛"
        }
    };

        var result = service.ValidateConfiguration(categories);

        Assert.False(result.IsValid);
    }
    [Fact]
    public void CategoryConfiguration_FinalCategoryWithMaximum_ShouldBeInvalid()
    {
        var service = CreateService();

        var categories = new List<VehicleCategory>
    {
        new()
        {
            Name = "Light",
            MinWeightKg = 0,
            MaxWeightKg = 500,
            Icon = "🚗"
        },
        new()
        {
            Name = "Medium",
            MinWeightKg = 500,
            MaxWeightKg = 2500,
            Icon = "🚚"
        },
        new()
        {
            Name = "Heavy",
            MinWeightKg = 2500,
            MaxWeightKg = 5000,
            Icon = "🚛"
        }
    };

        var result = service.ValidateConfiguration(categories);

        Assert.False(result.IsValid);
    }
}
