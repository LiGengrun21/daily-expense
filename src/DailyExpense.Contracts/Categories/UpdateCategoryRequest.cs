namespace DailyExpense.Contracts.Categories;

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? Color,
    string? Icon);
