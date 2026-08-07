namespace DailyExpense.Contracts.Statistics;

public sealed record TrendStatisticsResponse(
    int Months,
    IReadOnlyList<TrendStatisticsItem> Items);
