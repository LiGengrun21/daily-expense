using DailyExpense.Application.Reports;
using DailyExpense.Contracts.Dashboard;
using DailyExpense.Contracts.Expenses;
using DailyExpense.Contracts.Statistics;
using DailyExpense.Contracts.Summaries;
using DailyExpense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyExpense.Infrastructure.Reports;

public sealed class ReportService(DailyExpenseDbContext dbContext) : IReportService
{
    public async Task<MonthlySummaryResponse> GetMonthlySummaryAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var (from, to) = GetMonthRange(year, month);
        var totalExpenses = await GetTotalExpensesAsync(from, to, cancellationToken);
        var expenseCount = await GetExpenseCountAsync(from, to, cancellationToken);
        var totalBudget = await dbContext.MonthlyBudgets
            .AsNoTracking()
            .Where(budget => budget.Year == year && budget.Month == month && budget.CategoryId == null)
            .Select(budget => (decimal?)budget.Amount)
            .SingleOrDefaultAsync(cancellationToken);

        var categoryBreakdown = await GetCategoryBreakdownAsync(from, to, cancellationToken);
        var dailyBreakdown = await GetDailyBreakdownAsync(from, to, cancellationToken);
        var topCategories = categoryBreakdown.Take(5).ToList();

        return new MonthlySummaryResponse(
            year,
            month,
            totalExpenses,
            expenseCount,
            totalBudget,
            totalBudget is null ? null : totalBudget - totalExpenses,
            totalBudget is null or 0 ? null : Math.Round(totalExpenses / totalBudget.Value * 100, 2),
            categoryBreakdown,
            dailyBreakdown,
            topCategories);
    }

    public async Task<YearlySummaryResponse> GetYearlySummaryAsync(int year, CancellationToken cancellationToken)
    {
        var from = new DateOnly(year, 1, 1);
        var to = new DateOnly(year, 12, 31);
        var totalExpenses = await GetTotalExpensesAsync(from, to, cancellationToken);
        var expenseCount = await GetExpenseCountAsync(from, to, cancellationToken);
        var categoryBreakdown = await GetCategoryBreakdownAsync(from, to, cancellationToken);

        var monthlyRaw = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.ExpenseDate >= from && expense.ExpenseDate <= to)
            .GroupBy(expense => expense.ExpenseDate.Month)
            .Select(group => new
            {
                Month = group.Key,
                Amount = group.Sum(expense => expense.Amount),
                ExpenseCount = group.Count()
            })
            .ToListAsync(cancellationToken);

        var monthlyBreakdown = Enumerable.Range(1, 12)
            .Select(month =>
            {
                var item = monthlyRaw.SingleOrDefault(raw => raw.Month == month);
                return new MonthlyTotalItem(year, month, item?.Amount ?? 0m, item?.ExpenseCount ?? 0);
            })
            .ToList();

        return new YearlySummaryResponse(year, totalExpenses, expenseCount, monthlyBreakdown, categoryBreakdown);
    }

    public async Task<CategoryStatisticsResponse> GetCategoryStatisticsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var totalExpenses = await GetTotalExpensesAsync(from, to, cancellationToken);
        var categories = await GetCategoryBreakdownAsync(from, to, cancellationToken);
        var items = categories
            .Select(category => new CategoryStatisticsItem(
                category.CategoryId,
                category.CategoryName,
                category.Amount,
                category.ExpenseCount,
                category.Percentage))
            .ToList();

        return new CategoryStatisticsResponse(from, to, totalExpenses, items);
    }

    public async Task<DailyStatisticsResponse> GetDailyStatisticsAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var (from, to) = GetMonthRange(year, month);
        var daily = await GetDailyBreakdownAsync(from, to, cancellationToken);
        var items = daily
            .Select(day => new DailyStatisticsItem(day.Date, day.Amount, day.ExpenseCount))
            .ToList();

        return new DailyStatisticsResponse(year, month, items);
    }

    public async Task<TrendStatisticsResponse> GetTrendsAsync(int months, CancellationToken cancellationToken)
    {
        months = Math.Clamp(months, 1, 24);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonth = new DateOnly(today.Year, today.Month, 1);
        var firstMonth = currentMonth.AddMonths(-(months - 1));

        var raw = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.ExpenseDate >= firstMonth && expense.ExpenseDate < currentMonth.AddMonths(1))
            .GroupBy(expense => new { expense.ExpenseDate.Year, expense.ExpenseDate.Month })
            .Select(group => new
            {
                group.Key.Year,
                group.Key.Month,
                Amount = group.Sum(expense => expense.Amount),
                ExpenseCount = group.Count()
            })
            .ToListAsync(cancellationToken);

        var items = Enumerable.Range(0, months)
            .Select(offset =>
            {
                var month = firstMonth.AddMonths(offset);
                var match = raw.SingleOrDefault(item => item.Year == month.Year && item.Month == month.Month);
                return new TrendStatisticsItem(month.Year, month.Month, match?.Amount ?? 0m, match?.ExpenseCount ?? 0);
            })
            .ToList();

        return new TrendStatisticsResponse(months, items);
    }

    public async Task<DashboardResponse> GetDashboardAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var monthly = await GetMonthlySummaryAsync(year, month, cancellationToken);
        var (from, to) = GetMonthRange(year, month);

        var recentExpenses = await (
            from expense in dbContext.Expenses.AsNoTracking()
            join category in dbContext.Categories.AsNoTracking()
                on expense.CategoryId equals category.Id
            orderby expense.ExpenseDate descending, expense.CreatedAt descending
            select new ExpenseResponse(
                expense.Id,
                expense.Title,
                expense.Description,
                expense.Amount,
                expense.ExpenseDate,
                expense.CategoryId,
                category.Name,
                expense.PaymentMethod.ToString(),
                expense.CreatedAt,
                expense.UpdatedAt))
            .Take(5)
            .ToListAsync(cancellationToken);

        var categoryBudgets = await dbContext.MonthlyBudgets
            .AsNoTracking()
            .Where(budget => budget.Year == year && budget.Month == month && budget.CategoryId != null)
            .ToListAsync(cancellationToken);

        var overspentCategories = new List<CategorySummaryItem>();
        foreach (var budget in categoryBudgets)
        {
            var spent = monthly.CategoryBreakdown.SingleOrDefault(item => item.CategoryId == budget.CategoryId)?.Amount ?? 0m;
            if (spent > budget.Amount)
            {
                var category = monthly.CategoryBreakdown.Single(item => item.CategoryId == budget.CategoryId);
                overspentCategories.Add(category with
                {
                    Percentage = Math.Round(spent / budget.Amount * 100, 2)
                });
            }
        }

        return new DashboardResponse(
            year,
            month,
            monthly.TotalExpenses,
            monthly.TotalBudget,
            monthly.RemainingBudget,
            monthly.BudgetUsagePercentage,
            monthly.CategoryBreakdown,
            monthly.DailyBreakdown,
            recentExpenses,
            overspentCategories);
    }

    private async Task<decimal> GetTotalExpensesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.ExpenseDate >= from && expense.ExpenseDate <= to)
            .SumAsync(expense => (decimal?)expense.Amount, cancellationToken) ?? 0m;
    }

    private async Task<int> GetExpenseCountAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .CountAsync(expense => expense.ExpenseDate >= from && expense.ExpenseDate <= to, cancellationToken);
    }

    private async Task<IReadOnlyList<CategorySummaryItem>> GetCategoryBreakdownAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var startDate = from;
        var endDate = to;
        var totalExpenses = await GetTotalExpensesAsync(from, to, cancellationToken);

        var items = await (
            from expense in dbContext.Expenses.AsNoTracking()
            join category in dbContext.Categories.AsNoTracking()
                on expense.CategoryId equals category.Id
            where expense.ExpenseDate >= startDate && expense.ExpenseDate <= endDate
            group expense by new { expense.CategoryId, category.Name } into grouped
            orderby grouped.Sum(expense => expense.Amount) descending
            select new
            {
                grouped.Key.CategoryId,
                CategoryName = grouped.Key.Name,
                Amount = grouped.Sum(expense => expense.Amount),
                ExpenseCount = grouped.Count()
            })
            .ToListAsync(cancellationToken);

        return items
            .Select(item => new CategorySummaryItem(
                item.CategoryId,
                item.CategoryName,
                item.Amount,
                item.ExpenseCount,
                totalExpenses == 0 ? 0 : Math.Round(item.Amount / totalExpenses * 100, 2)))
            .ToList();
    }

    private async Task<IReadOnlyList<DailySummaryItem>> GetDailyBreakdownAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken)
    {
        var raw = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.ExpenseDate >= from && expense.ExpenseDate <= to)
            .GroupBy(expense => expense.ExpenseDate)
            .Select(group => new
            {
                Date = group.Key,
                Amount = group.Sum(expense => expense.Amount),
                ExpenseCount = group.Count()
            })
            .ToListAsync(cancellationToken);

        var days = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1)
            .Select(offset =>
            {
                var date = from.AddDays(offset);
                var item = raw.SingleOrDefault(rawItem => rawItem.Date == date);
                return new DailySummaryItem(date, item?.Amount ?? 0m, item?.ExpenseCount ?? 0);
            })
            .ToList();

        return days;
    }

    private static (DateOnly From, DateOnly To) GetMonthRange(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        return (from, from.AddMonths(1).AddDays(-1));
    }
}
