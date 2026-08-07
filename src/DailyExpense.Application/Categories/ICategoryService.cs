using DailyExpense.Contracts.Categories;

namespace DailyExpense.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(CancellationToken cancellationToken);

    Task<CategoryResponse?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<CategoryServiceResult<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken);

    Task<CategoryServiceResult<CategoryResponse>> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken);

    Task<CategoryServiceResult<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
}
