using DailyExpense.Contracts.Dashboard;
using DailyExpense.Contracts.Statistics;
using DailyExpense.Contracts.Summaries;

namespace DailyExpense.Application.Reports;

public interface IReportService
{
    Task<MonthlySummaryResponse> GetMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken);

    Task<YearlySummaryResponse> GetYearlySummaryAsync(int year, CancellationToken cancellationToken);

    Task<CategoryStatisticsResponse> GetCategoryStatisticsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken);

    Task<DailyStatisticsResponse> GetDailyStatisticsAsync(int year, int month, CancellationToken cancellationToken);

    Task<TrendStatisticsResponse> GetTrendsAsync(int months, CancellationToken cancellationToken);

    Task<DashboardResponse> GetDashboardAsync(int year, int month, CancellationToken cancellationToken);
}
