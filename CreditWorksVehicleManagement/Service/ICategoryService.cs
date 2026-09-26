using CreditWorksVehicleManagement.Models;

namespace CreditWorksVehicleManagement.Services;

public interface ICategoryService
{
    Task<List<VehicleCategory>> GetCategoriesAsync();

    Task<VehicleCategory?> GetCategoryForWeightAsync(decimal weightKg);

    (bool IsValid, string? ErrorMessage) ValidateConfiguration(
        IEnumerable<VehicleCategory> categories);

    Task<(bool Success, string? ErrorMessage)> CreateCategoryAsync(
        VehicleCategory category);

    Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(
        VehicleCategory category);

    Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(
        int categoryId);
}