namespace DailyExpense.Contracts.Summaries;

public sealed record DailySummaryItem(
    DateOnly Date,
    decimal Amount,
    int ExpenseCount);
