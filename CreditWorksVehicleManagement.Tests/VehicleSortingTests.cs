using CreditWorksVehicleManagement.Controllers;
using CreditWorksVehicleManagement.Data;
using CreditWorksVehicleManagement.Models;
using CreditWorksVehicleManagement.Services;
using CreditWorksVehicleManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CreditWorksVehicleManagement.Tests;

public class VehicleSortingTests
{
    private static (ApplicationDbContext Context, VehiclesController Controller)
        CreateController()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);

        context.Manufacturers.AddRange(
            new Manufacturer { Id = 1, Name = "Mazda" },
            new Manufacturer { Id = 2, Name = "Honda" },
            new Manufacturer { Id = 3, Name = "Ferrari" }
        );

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
            }
        );

        context.Vehicles.AddRange(
            new Vehicle
            {
                Id = 1,
                OwnerName = "Charlie",
                ManufacturerId = 1,
                YearOfManufacture = 2020,
                WeightKg = 1200
            },
            new Vehicle
            {
                Id = 2,
                OwnerName = "Alice",
                ManufacturerId = 2,
                YearOfManufacture = 2015,
                WeightKg = 400
            },
            new Vehicle
            {
                Id = 3,
                OwnerName = "Bob",
                ManufacturerId = 3,
                YearOfManufacture = 2025,
                WeightKg = 2800
            }
        );

        context.SaveChanges();

        var categoryService = new CategoryService(context);

        var controller = new VehiclesController(
            context,
            categoryService);

        return (context, controller);
    }

    private static async Task<VehicleListViewModel> GetModel(
        VehiclesController controller,
        string sortBy,
        string sortDirection)
    {
        var result = await controller.Index(
            sortBy,
            sortDirection);

        var viewResult = Assert.IsType<ViewResult>(result);

        return Assert.IsType<VehicleListViewModel>(
            viewResult.Model);
    }

    [Fact]
    public async Task SortByOwnerAscending_ShouldReturnCorrectOrder()
    {
        var (_, controller) = CreateController();

        var model = await GetModel(
            controller,
            "Owner",
            "asc");

        Assert.Equal(
            new[] { "Alice", "Bob", "Charlie" },
            model.Vehicles.Select(v => v.OwnerName));
    }

    [Fact]
    public async Task SortByOwnerDescending_ShouldReturnCorrectOrder()
    {
        var (_, controller) = CreateController();

        var model = await GetModel(
            controller,
            "Owner",
            "desc");

        Assert.Equal(
            new[] { "Charlie", "Bob", "Alice" },
            model.Vehicles.Select(v => v.OwnerName));
    }

    [Fact]
    public async Task SortByManufacturerAscending_ShouldReturnCorrectOrder()
    {
        var (_, controller) = CreateController();

        var model = await GetModel(
            controller,
            "Manufacturer",
            "asc");

        Assert.Equal(
            new[] { "Ferrari", "Honda", "Mazda" },
            model.Vehicles.Select(v => v.ManufacturerName));
    }

    [Fact]
    public async Task SortByYearAscending_ShouldReturnCorrectOrder()
    {
        var (_, controller) = CreateController();

        var model = await GetModel(
            controller,
            "Year",
            "asc");

        Assert.Equal(
            new[] { 2015, 2020, 2025 },
            model.Vehicles.Select(v => v.YearOfManufacture));
    }

    [Fact]
    public async Task SortByWeightDescending_ShouldReturnCorrectOrder()
    {
        var (_, controller) = CreateController();

        var model = await GetModel(
            controller,
            "Weight",
            "desc");

        Assert.Equal(
            new[] { 2800m, 1200m, 400m },
            model.Vehicles.Select(v => v.WeightKg));
    }
}