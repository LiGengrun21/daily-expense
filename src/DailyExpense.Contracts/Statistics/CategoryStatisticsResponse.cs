namespace DailyExpense.Contracts.Statistics;

public sealed record CategoryStatisticsResponse(
    DateOnly From,
    DateOnly To,
    decimal TotalExpenses,
    IReadOnlyList<CategoryStatisticsItem> Categories);
