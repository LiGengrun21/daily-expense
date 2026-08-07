namespace DailyExpense.Contracts.Summaries;

public sealed record YearlySummaryResponse(
    int Year,
    decimal TotalExpenses,
    int ExpenseCount,
    IReadOnlyList<MonthlyTotalItem> MonthlyBreakdown,
    IReadOnlyList<CategorySummaryItem> CategoryBreakdown);
