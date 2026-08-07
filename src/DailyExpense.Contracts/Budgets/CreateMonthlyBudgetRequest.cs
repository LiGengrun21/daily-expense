namespace DailyExpense.Contracts.Budgets;

public sealed record CreateMonthlyBudgetRequest(
    int Year,
    int Month,
    decimal Amount,
    Guid? CategoryId);
