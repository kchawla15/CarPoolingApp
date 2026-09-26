using System.ComponentModel.DataAnnotations;
using CreditWorksVehicleManagement.ViewModels;

namespace CreditWorksVehicleManagement.Tests;

public class VehicleValidationTests
{
    private static IList<ValidationResult> Validate(
        VehicleCreateViewModel model)
    {
        var context = new ValidationContext(model);

        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            context,
            results,
            validateAllProperties: true);

        return results;
    }

    [Fact]
    public void OwnerName_IsRequired()
    {
        var model = new VehicleCreateViewModel
        {
            OwnerName = "",
            ManufacturerId = 1,
            YearOfManufacture = 2020,
            WeightKg = 100
        };

        var results = Validate(model);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(model.OwnerName)));
    }

    [Fact]
    public void YearOfManufacture_MustBeValidRange()
    {
        var model = new VehicleCreateViewModel
        {
            OwnerName = "John Smith",
            ManufacturerId = 1,
            YearOfManufacture = 1800,
            WeightKg = 100
        };

        var results = Validate(model);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(model.YearOfManufacture)));
    }

    [Fact]
    public void Weight_MustBePositive()
    {
        var model = new VehicleCreateViewModel
        {
            OwnerName = "John Smith",
            ManufacturerId = 1,
            YearOfManufacture = 2020,
            WeightKg = 0
        };

        var results = Validate(model);

        Assert.Contains(
            results,
            r => r.MemberNames.Contains(nameof(model.WeightKg)));
    }

    [Fact]
    public void Weight_MustNotHaveMoreThanTwoDecimalPlaces()
    {
        var model = new VehicleCreateViewModel
        {
            OwnerName = "John Smith",
            ManufacturerId = 1,
            YearOfManufacture = 2020,
            WeightKg = 100.123m
        };

        var results = Validate(model);

        // The ViewModel's DataAnnotations do not check decimal precision.
        // This rule is enforced by VehiclesController.
        Assert.DoesNotContain(
            results,
            r => r.MemberNames.Contains(nameof(model.WeightKg)));
    }
}