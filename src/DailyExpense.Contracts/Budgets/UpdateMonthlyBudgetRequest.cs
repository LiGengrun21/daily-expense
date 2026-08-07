namespace DailyExpense.Contracts.Budgets;

public sealed record UpdateMonthlyBudgetRequest(
    int Year,
    int Month,
    decimal Amount,
    Guid? CategoryId);
