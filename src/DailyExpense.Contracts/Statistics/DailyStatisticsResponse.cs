namespace DailyExpense.Contracts.Statistics;

public sealed record DailyStatisticsResponse(
    int Year,
    int Month,
    IReadOnlyList<DailyStatisticsItem> Days);
