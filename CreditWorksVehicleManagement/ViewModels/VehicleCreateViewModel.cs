using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CreditWorksVehicleManagement.ViewModels;

public class VehicleCreateViewModel
{
    [Required]
    [Display(Name = "Owner Name")]
    public string OwnerName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Manufacturer")]
    public int ManufacturerId { get; set; }

    [Required]
    [Range(1900, 2100)]
    [Display(Name = "Year of Manufacture")]
    public int YearOfManufacture { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "999999999.99")]
    [Display(Name = "Weight (kg)")]
    public decimal WeightKg { get; set; }

    public IEnumerable<SelectListItem> Manufacturers { get; set; }
        = new List<SelectListItem>();
}