using DailyExpense.Application.Categories;
using DailyExpense.Contracts.Categories;
using DailyExpense.Domain.Categories;
using DailyExpense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyExpense.Infrastructure.Categories;

public sealed class CategoryService(DailyExpenseDbContext dbContext) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .OrderByDescending(category => category.IsDefault)
            .ThenBy(category => category.Name)
            .Select(category => ToResponse(category))
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .Where(category => category.Id == id)
            .Select(category => ToResponse(category))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<CategoryServiceResult<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateCategoryInputAsync(
            request.Name,
            existingCategoryId: null,
            cancellationToken);

        if (validationError is not null)
        {
            return CategoryServiceResult<CategoryResponse>.Failure(validationError.Value.Code, validationError.Value.Message);
        }

        var category = new Category(
            request.Name,
            request.Description,
            request.Color,
            request.Icon);

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CategoryServiceResult<CategoryResponse>.Success(ToResponse(category));
    }

    public async Task<CategoryServiceResult<CategoryResponse>> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.SingleOrDefaultAsync(
            category => category.Id == id,
            cancellationToken);

        if (category is null)
        {
            return CategoryServiceResult<CategoryResponse>.Failure("Category.NotFound", "Category was not found.");
        }

        var validationError = await ValidateCategoryInputAsync(
            request.Name,
            existingCategoryId: id,
            cancellationToken);

        if (validationError is not null)
        {
            return CategoryServiceResult<CategoryResponse>.Failure(validationError.Value.Code, validationError.Value.Message);
        }

        category.Rename(request.Name);
        category.UpdateDetails(request.Description, request.Color, request.Icon);

        await dbContext.SaveChangesAsync(cancellationToken);

        return CategoryServiceResult<CategoryResponse>.Success(ToResponse(category));
    }

    public async Task<CategoryServiceResult<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.SingleOrDefaultAsync(
            category => category.Id == id,
            cancellationToken);

        if (category is null)
        {
            return CategoryServiceResult<bool>.Failure("Category.NotFound", "Category was not found.");
        }

        if (category.IsDefault)
        {
            return CategoryServiceResult<bool>.Failure("Category.DefaultCannotBeDeleted", "Default categories cannot be deleted.");
        }

        var hasExpenses = await dbContext.Expenses.AnyAsync(
            expense => expense.CategoryId == id,
            cancellationToken);

        if (hasExpenses)
        {
            return CategoryServiceResult<bool>.Failure("Category.InUse", "Category cannot be deleted because it has expenses.");
        }

        dbContext.Categories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CategoryServiceResult<bool>.Success(true);
    }

    private async Task<(string Code, string Message)?> ValidateCategoryInputAsync(
        string name,
        Guid? existingCategoryId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ("Category.NameRequired", "Category name is required.");
        }

        var normalizedName = name.Trim();
        var nameExists = await dbContext.Categories.AnyAsync(
            category => category.Name == normalizedName
                && (existingCategoryId == null || category.Id != existingCategoryId.Value),
            cancellationToken);

        if (nameExists)
        {
            return ("Category.NameAlreadyExists", "Category name already exists.");
        }

        return null;
    }

    private static CategoryResponse ToResponse(Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.Color,
            category.Icon,
            category.IsDefault,
            category.CreatedAt,
            category.UpdatedAt);
    }
}
