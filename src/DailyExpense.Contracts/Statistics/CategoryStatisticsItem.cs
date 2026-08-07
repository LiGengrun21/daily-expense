namespace DailyExpense.Contracts.Statistics;

public sealed record CategoryStatisticsItem(
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    int ExpenseCount,
    decimal Percentage);
