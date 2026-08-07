namespace DailyExpense.Contracts.Categories;

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? Color,
    string? Icon,
    bool IsDefault,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
