namespace DailyExpense.Contracts.Budgets;

public sealed record MonthlyBudgetResponse(
    Guid Id,
    int Year,
    int Month,
    decimal Amount,
    Guid? CategoryId,
    string? CategoryName,
    decimal SpentAmount,
    decimal RemainingAmount,
    decimal UsagePercentage,
    bool IsOverBudget,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
