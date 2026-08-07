using DailyExpense.Domain.Budgets;

namespace DailyExpense.UnitTests;

public sealed class MonthlyBudgetTests
{
    [Fact]
    public void Constructor_requires_valid_month()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MonthlyBudget(2026, 13, 500));
    }

    [Fact]
    public void Constructor_requires_positive_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MonthlyBudget(2026, 8, 0));
    }

    [Fact]
    public void Constructor_allows_total_monthly_budget()
    {
        var budget = new MonthlyBudget(2026, 8, 1500);

        Assert.Null(budget.CategoryId);
        Assert.Null(budget.UpdatedAt);
    }
}
