using CreditWorksVehicleManagement.Data;
using CreditWorksVehicleManagement.Models;
using CreditWorksVehicleManagement.Services;
using CreditWorksVehicleManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CreditWorksVehicleManagement.Controllers;

public class VehiclesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICategoryService _categoryService;

    public VehiclesController(
        ApplicationDbContext context,
        ICategoryService categoryService)
    {
        _context = context;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(
        string sortBy = "Owner",
        string sortDirection = "asc")
    {
        var vehicles = await _context.Vehicles
            .Include(v => v.Manufacturer)
            .ToListAsync();

        var categories = await _categoryService
            .GetCategoriesAsync();

        var vehicleItems = vehicles
            .Select(vehicle =>
            {
                var category = categories.SingleOrDefault(c =>
                    vehicle.WeightKg >= c.MinWeightKg &&
                    (c.MaxWeightKg == null ||
                     vehicle.WeightKg < c.MaxWeightKg.Value));

                return new VehicleListItemViewModel
                {
                    Id = vehicle.Id,
                    OwnerName = vehicle.OwnerName,
                    ManufacturerName = vehicle.Manufacturer.Name,
                    YearOfManufacture = vehicle.YearOfManufacture,
                    WeightKg = vehicle.WeightKg,
                    CategoryName = category?.Name ?? "Uncategorised",
                    CategoryIcon = category?.Icon ?? "?"
                };
            })
            .ToList();

        sortBy = sortBy switch
        {
            "Owner" => "Owner",
            "Manufacturer" => "Manufacturer",
            "Year" => "Year",
            "Weight" => "Weight",
            _ => "Owner"
        };

        sortDirection = sortDirection.Equals(
            "desc",
            StringComparison.OrdinalIgnoreCase)
            ? "desc"
            : "asc";

        vehicleItems = sortBy switch
        {
            "Manufacturer" => sortDirection == "asc"
                ? vehicleItems.OrderBy(v => v.ManufacturerName).ToList()
                : vehicleItems.OrderByDescending(v => v.ManufacturerName).ToList(),

            "Year" => sortDirection == "asc"
                ? vehicleItems.OrderBy(v => v.YearOfManufacture).ToList()
                : vehicleItems.OrderByDescending(v => v.YearOfManufacture).ToList(),

            "Weight" => sortDirection == "asc"
                ? vehicleItems.OrderBy(v => v.WeightKg).ToList()
                : vehicleItems.OrderByDescending(v => v.WeightKg).ToList(),

            _ => sortDirection == "asc"
                ? vehicleItems.OrderBy(v => v.OwnerName).ToList()
                : vehicleItems.OrderByDescending(v => v.OwnerName).ToList()
        };

        var viewModel = new VehicleListViewModel
        {
            Vehicles = vehicleItems,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new VehicleCreateViewModel();

        await PopulateManufacturersAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleCreateViewModel viewModel)
    {
        if (string.IsNullOrWhiteSpace(viewModel.OwnerName))
        {
            ModelState.AddModelError(
                nameof(viewModel.OwnerName),
                "Owner name is required.");
        }

        if (viewModel.YearOfManufacture > DateTime.Now.Year)
        {
            ModelState.AddModelError(
                nameof(viewModel.YearOfManufacture),
                "Year of manufacture cannot be in the future.");
        }

        if (viewModel.WeightKg != decimal.Round(viewModel.WeightKg, 2))
        {
            ModelState.AddModelError(
                nameof(viewModel.WeightKg),
                "Weight can have a maximum of 2 decimal places.");
        }

        if (viewModel.WeightKg <= 0)
        {
            ModelState.AddModelError(
                nameof(viewModel.WeightKg),
                "Weight must be greater than zero.");
        }

        var manufacturerExists = await _context.Manufacturers
            .AnyAsync(m => m.Id == viewModel.ManufacturerId);

        if (!manufacturerExists)
        {
            ModelState.AddModelError(
                nameof(viewModel.ManufacturerId),
                "Please select a valid manufacturer.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateManufacturersAsync(viewModel);
            return View(viewModel);
        }

        var category = await _categoryService
            .GetCategoryForWeightAsync(viewModel.WeightKg);

        if (category == null)
        {
            ModelState.AddModelError(
                string.Empty,
                "The vehicle weight does not belong to a valid category configuration.");

            await PopulateManufacturersAsync(viewModel);
            return View(viewModel);
        }

        var vehicle = new Vehicle
        {
            OwnerName = viewModel.OwnerName.Trim(),
            ManufacturerId = viewModel.ManufacturerId,
            YearOfManufacture = viewModel.YearOfManufacture,
            WeightKg = viewModel.WeightKg
        };

        _context.Vehicles.Add(vehicle);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateManufacturersAsync(
        VehicleCreateViewModel viewModel)
    {
        viewModel.Manufacturers = await _context.Manufacturers
            .OrderBy(m => m.Name)
            .Select(m => new SelectListItem
            {
                Value = m.Id.ToString(),
                Text = m.Name
            })
            .ToListAsync();
    }
}