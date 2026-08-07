namespace DailyExpense.Contracts.Statistics;

public sealed record TrendStatisticsItem(
    int Year,
    int Month,
    decimal Amount,
    int ExpenseCount);
