namespace DailyExpense.Contracts.Statistics;

public sealed record DailyStatisticsItem(
    DateOnly Date,
    decimal Amount,
    int ExpenseCount);
