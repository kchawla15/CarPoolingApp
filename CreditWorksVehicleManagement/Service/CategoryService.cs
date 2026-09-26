using CreditWorksVehicleManagement.Data;
using CreditWorksVehicleManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CreditWorksVehicleManagement.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleCategory>> GetCategoriesAsync()
    {
        return await _context.VehicleCategories
            .OrderBy(c => c.MinWeightKg)
            .ToListAsync();
    }

    public async Task<VehicleCategory?> GetCategoryForWeightAsync(decimal weightKg)
    {
        if (weightKg < 0)
        {
            return null;
        }

        return await _context.VehicleCategories
            .Where(c =>
                weightKg >= c.MinWeightKg &&
                (c.MaxWeightKg == null || weightKg < c.MaxWeightKg))
            .SingleOrDefaultAsync();
    }

    public (bool IsValid, string? ErrorMessage) ValidateConfiguration(
        IEnumerable<VehicleCategory> categories)
    {
        var categoryList = categories
            .OrderBy(c => c.MinWeightKg)
            .ToList();

        if (!categoryList.Any())
        {
            return (false, "At least one category is required.");
        }

        foreach (var category in categoryList)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return (false, "Category name is required.");
            }

            if (string.IsNullOrWhiteSpace(category.Icon))
            {
                return (false, "Category icon is required.");
            }

            if (category.MinWeightKg < 0)
            {
                return (false, "Minimum weight cannot be negative.");
            }

            if (category.MaxWeightKg.HasValue &&
                category.MaxWeightKg.Value <= category.MinWeightKg)
            {
                return (
                    false,
                    $"Maximum weight for '{category.Name}' must be greater than its minimum weight.");
            }
        }

        if (categoryList[0].MinWeightKg != 0)
        {
            return (
                false,
                "The first category must start at 0 kg so that all valid vehicle weights are covered.");
        }

        for (int i = 0; i < categoryList.Count - 1; i++)
        {
            var current = categoryList[i];
            var next = categoryList[i + 1];

            if (!current.MaxWeightKg.HasValue)
            {
                return (
                    false,
                    $"Category '{current.Name}' must have a maximum weight because it is not the final category.");
            }

            if (current.MaxWeightKg.Value != next.MinWeightKg)
            {
                return (
                    false,
                    $"There is a gap or overlap between '{current.Name}' and '{next.Name}'.");
            }
        }

        var finalCategory = categoryList[^1];

        if (finalCategory.MaxWeightKg.HasValue)
        {
            return (
                false,
                $"The final category '{finalCategory.Name}' must have no maximum weight.");
        }

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateCategoryAsync(
        VehicleCategory category)
    {
        var categories = await GetCategoriesAsync();

        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return (false, "Category name is required.");
        }

        if (string.IsNullOrWhiteSpace(category.Icon))
        {
            return (false, "Category icon is required.");
        }

        if (category.MinWeightKg < 0)
        {
            return (false, "Minimum weight cannot be negative.");
        }

        if (category.MaxWeightKg.HasValue &&
            category.MaxWeightKg.Value <= category.MinWeightKg)
        {
            return (
                false,
                "Maximum weight must be greater than minimum weight.");
        }

        // Find the existing category containing the new minimum.
        var containingCategory = categories.FirstOrDefault(c =>
            category.MinWeightKg >= c.MinWeightKg &&
            (c.MaxWeightKg == null ||
             category.MinWeightKg < c.MaxWeightKg.Value));

        if (containingCategory == null)
        {
            return (
                false,
                "The new category minimum must fall within an existing category range.");
        }

        // Creating a category without a maximum creates a new
        // final category.
        //
        // Example:
        //
        // Heavy: 2500+
        //
        // Create:
        // Extra Heavy: 3500+
        //
        // Result:
        // Heavy:       2500-3500
        // Extra Heavy: 3500+

        if (!category.MaxWeightKg.HasValue)
        {
            if (containingCategory.MaxWeightKg.HasValue)
            {
                return (
                    false,
                    "A category without a maximum weight must be the final category.");
            }

            if (category.MinWeightKg <= containingCategory.MinWeightKg)
            {
                return (
                    false,
                    "The new final category must start above the existing final category's minimum weight.");
            }

            containingCategory.MaxWeightKg = category.MinWeightKg;

            var newFinalCategory = new VehicleCategory
            {
                Name = category.Name.Trim(),
                MinWeightKg = category.MinWeightKg,
                MaxWeightKg = null,
                Icon = category.Icon
            };

            categories.Add(newFinalCategory);

            var validation = ValidateConfiguration(categories);

            if (!validation.IsValid)
            {
                return (false, validation.ErrorMessage);
            }

            _context.VehicleCategories.Add(newFinalCategory);

            await _context.SaveChangesAsync();

            return (true, null);
        }

        // A finite category must start at the minimum boundary
        // of an existing category.
        if (category.MinWeightKg != containingCategory.MinWeightKg)
        {
            return (
                false,
                "A new category with a maximum weight must start at the minimum boundary of an existing category.");
        }

        // The new category must fit inside the existing category.
        if (containingCategory.MaxWeightKg.HasValue &&
            category.MaxWeightKg.Value >= containingCategory.MaxWeightKg.Value)
        {
            return (
                false,
                "The new category maximum must be inside the existing category range.");
        }

        // Example:
        //
        // Heavy: 2500+
        //
        // Create:
        // Very Heavy: 2500-3000
        //
        // Result:
        // Very Heavy: 2500-3000
        // Heavy:      3000+
        var newCategory = new VehicleCategory
        {
            Name = category.Name.Trim(),
            MinWeightKg = category.MinWeightKg,
            MaxWeightKg = category.MaxWeightKg,
            Icon = category.Icon
        };

        containingCategory.MinWeightKg = category.MaxWeightKg.Value;

        categories.Add(newCategory);

        var finalValidation = ValidateConfiguration(categories);

        if (!finalValidation.IsValid)
        {
            return (false, finalValidation.ErrorMessage);
        }

        _context.VehicleCategories.Add(newCategory);

        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateCategoryAsync(
        VehicleCategory category)
    {
        var categories = await GetCategoriesAsync();

        var orderedCategories = categories
            .OrderBy(c => c.MinWeightKg)
            .ToList();

        var categoryIndex = orderedCategories.FindIndex(
            c => c.Id == category.Id);

        if (categoryIndex < 0)
        {
            return (false, "The category could not be found.");
        }

        // Build a candidate configuration so invalid edits never leave tracked
        // entities modified in the current DbContext.
        var updatedCategories = orderedCategories
            .Select(c => new VehicleCategory
            {
                Id = c.Id,
                Name = c.Name,
                MinWeightKg = c.MinWeightKg,
                MaxWeightKg = c.MaxWeightKg,
                Icon = c.Icon
            })
            .ToList();

        var updatedCategory = updatedCategories[categoryIndex];

        var previousCategory = categoryIndex > 0
            ? updatedCategories[categoryIndex - 1]
            : null;

        var nextCategory = categoryIndex < updatedCategories.Count - 1
            ? updatedCategories[categoryIndex + 1]
            : null;

        var minimumChanged =
            category.MinWeightKg != updatedCategory.MinWeightKg;

        var maximumChanged =
            category.MaxWeightKg != updatedCategory.MaxWeightKg;

        updatedCategory.Name = category.Name;
        updatedCategory.MinWeightKg = category.MinWeightKg;
        updatedCategory.MaxWeightKg = category.MaxWeightKg;
        updatedCategory.Icon = category.Icon;

        // A category's minimum is shared with the previous category's maximum;
        // its maximum is shared with the next category's minimum.
        if (minimumChanged && previousCategory != null)
        {
            previousCategory.MaxWeightKg = category.MinWeightKg;
        }

        if (maximumChanged &&
            nextCategory != null &&
            category.MaxWeightKg.HasValue)
        {
            nextCategory.MinWeightKg = category.MaxWeightKg.Value;
        }

        var validation = ValidateConfiguration(updatedCategories);

        if (!validation.IsValid)
        {
            return (false, validation.ErrorMessage);
        }

        var updatedById = updatedCategories.ToDictionary(c => c.Id);

        foreach (var existingCategory in categories)
        {
            var updated = updatedById[existingCategory.Id];

            existingCategory.Name = updated.Name;
            existingCategory.MinWeightKg = updated.MinWeightKg;
            existingCategory.MaxWeightKg = updated.MaxWeightKg;
            existingCategory.Icon = updated.Icon;
        }

        await _context.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(
        int categoryId)
    {
        var categories = await GetCategoriesAsync();

        var categoryIndex = categories.FindIndex(
            c => c.Id == categoryId);

        if (categoryIndex < 0)
        {
            return (false, "The category could not be found.");
        }

        if (categories.Count == 1)
        {
            return (
                false,
                "At least one category is required.");
        }

        var category = categories[categoryIndex];

        var previousCategory = categoryIndex > 0
            ? categories[categoryIndex - 1]
            : null;

        var nextCategory = categoryIndex < categories.Count - 1
            ? categories[categoryIndex + 1]
            : null;

        if (previousCategory != null)
        {
            // Merge the deleted category's range into the
            // previous category.
            //
            // Example:
            //
            // Light:  0-500
            // Medium: 500-2500
            // Heavy:  2500+
            //
            // Delete Medium:
            //
            // Light:  0-2500
            // Heavy:  2500+

            previousCategory.MaxWeightKg =
                category.MaxWeightKg;
        }
        else if (nextCategory != null)
        {
            // If deleting the first category, move the
            // next category's minimum to zero.
            //
            // Example:
            //
            // Light:  0-500
            // Medium: 500-2500
            //
            // Delete Light:
            //
            // Medium: 0-2500

            nextCategory.MinWeightKg = 0;
        }

        categories.RemoveAt(categoryIndex);

        var validation = ValidateConfiguration(categories);

        if (!validation.IsValid)
        {
            return (false, validation.ErrorMessage);
        }

        _context.VehicleCategories.Remove(category);

        await _context.SaveChangesAsync();

        return (true, null);
    }
}