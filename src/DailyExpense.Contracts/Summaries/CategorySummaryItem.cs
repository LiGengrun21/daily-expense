namespace DailyExpense.Contracts.Summaries;

public sealed record CategorySummaryItem(
    Guid CategoryId,
    string CategoryName,
    decimal Amount,
    int ExpenseCount,
    decimal Percentage);
