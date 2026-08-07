namespace DailyExpense.Contracts.Expenses;

public sealed record ExpenseResponse(
    Guid Id,
    string Title,
    string? Description,
    decimal Amount,
    DateOnly ExpenseDate,
    Guid CategoryId,
    string CategoryName,
    string PaymentMethod,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
