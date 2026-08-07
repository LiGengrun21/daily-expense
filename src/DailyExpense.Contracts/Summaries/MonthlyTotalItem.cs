namespace DailyExpense.Contracts.Summaries;

public sealed record MonthlyTotalItem(
    int Year,
    int Month,
    decimal Amount,
    int ExpenseCount);
