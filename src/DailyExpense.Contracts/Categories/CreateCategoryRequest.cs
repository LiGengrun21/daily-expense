namespace DailyExpense.Contracts.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    string? Description,
    string? Color,
    string? Icon);
