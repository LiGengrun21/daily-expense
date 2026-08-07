using DailyExpense.Contracts.Expenses;
using DailyExpense.Contracts.Summaries;

namespace DailyExpense.Contracts.Dashboard;

public sealed record DashboardResponse(
    int Year,
    int Month,
    decimal MonthlyTotal,
    decimal? TotalBudget,
    decimal? RemainingBudget,
    decimal? BudgetUsagePercentage,
    IReadOnlyList<CategorySummaryItem> CategoryStats,
    IReadOnlyList<DailySummaryItem> DailySpending,
    IReadOnlyList<ExpenseResponse> RecentExpenses,
    IReadOnlyList<CategorySummaryItem> OverspentCategories);
