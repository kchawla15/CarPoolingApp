using CreditWorksVehicleManagement.Models;

namespace CreditWorksVehicleManagement.ViewModels;

public class VehicleListViewModel
{
    public List<VehicleListItemViewModel> Vehicles { get; set; } = new();

    public string SortBy { get; set; } = "Owner";

    public string SortDirection { get; set; } = "asc";
}

public class VehicleListItemViewModel
{
    public int Id { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public string ManufacturerName { get; set; } = string.Empty;

    public int YearOfManufacture { get; set; }

    public decimal WeightKg { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string CategoryIcon { get; set; } = string.Empty;
}