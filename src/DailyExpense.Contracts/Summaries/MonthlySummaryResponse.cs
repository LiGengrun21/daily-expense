namespace DailyExpense.Contracts.Summaries;

public sealed record MonthlySummaryResponse(
    int Year,
    int Month,
    decimal TotalExpenses,
    int ExpenseCount,
    decimal? TotalBudget,
    decimal? RemainingBudget,
    decimal? BudgetUsagePercentage,
    IReadOnlyList<CategorySummaryItem> CategoryBreakdown,
    IReadOnlyList<DailySummaryItem> DailyBreakdown,
    IReadOnlyList<CategorySummaryItem> TopCategories);
