namespace DailyExpense.Contracts.Expenses;

public sealed record CreateExpenseRequest(
    string Title,
    decimal Amount,
    DateOnly ExpenseDate,
    Guid CategoryId,
    string? PaymentMethod,
    string? Description);
