using DailyExpense.Application.Reports;

namespace DailyExpense.Api.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/summaries/monthly", async (
            IReportService reportService,
            int year,
            int month,
            CancellationToken cancellationToken) =>
        {
            if (!IsValidMonth(year, month))
            {
                return Results.BadRequest("Year must be between 2000 and 2100, and month must be between 1 and 12.");
            }

            var result = await reportService.GetMonthlySummaryAsync(year, month, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Summaries");

        endpoints.MapGet("/api/v1/summaries/yearly", async (
            IReportService reportService,
            int year,
            CancellationToken cancellationToken) =>
        {
            if (!IsValidYear(year))
            {
                return Results.BadRequest("Year must be between 2000 and 2100.");
            }

            var result = await reportService.GetYearlySummaryAsync(year, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Summaries");

        endpoints.MapGet("/api/v1/statistics/by-category", async (
            IReportService reportService,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken) =>
        {
            if (from > to)
            {
                return Results.BadRequest("'from' must be earlier than or equal to 'to'.");
            }

            var result = await reportService.GetCategoryStatisticsAsync(from, to, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Statistics");

        endpoints.MapGet("/api/v1/statistics/by-day", async (
            IReportService reportService,
            int year,
            int month,
            CancellationToken cancellationToken) =>
        {
            if (!IsValidMonth(year, month))
            {
                return Results.BadRequest("Year must be between 2000 and 2100, and month must be between 1 and 12.");
            }

            var result = await reportService.GetDailyStatisticsAsync(year, month, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Statistics");

        endpoints.MapGet("/api/v1/statistics/trends", async (
            IReportService reportService,
            int? months,
            CancellationToken cancellationToken) =>
        {
            var result = await reportService.GetTrendsAsync(months.GetValueOrDefault(6), cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Statistics");

        endpoints.MapGet("/api/v1/dashboard", async (
            IReportService reportService,
            int year,
            int month,
            CancellationToken cancellationToken) =>
        {
            if (!IsValidMonth(year, month))
            {
                return Results.BadRequest("Year must be between 2000 and 2100, and month must be between 1 and 12.");
            }

            var result = await reportService.GetDashboardAsync(year, month, cancellationToken);
            return Results.Ok(result);
        })
        .WithTags("Dashboard");

        return endpoints;
    }

    private static bool IsValidYear(int year)
    {
        return year is >= 2000 and <= 2100;
    }

    private static bool IsValidMonth(int year, int month)
    {
        return IsValidYear(year) && month is >= 1 and <= 12;
    }
}
