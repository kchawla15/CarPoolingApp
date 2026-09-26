namespace CreditWorksVehicleManagement.Models;

public class VehicleCategory
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal MinWeightKg { get; set; }

    public decimal? MaxWeightKg { get; set; }

    public string Icon { get; set; } = string.Empty;
}