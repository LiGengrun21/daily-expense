using DailyExpense.Application.Budgets;
using DailyExpense.Domain.Budgets;

namespace DailyExpense.UnitTests;

public sealed class BudgetAllocationValidatorTests
{
    [Theory]
    [InlineData(399, true)]
    [InlineData(400, true)]
    [InlineData(401, false)]
    public void Adding_category_checks_remaining_allocation(decimal amount, bool allowed)
    {
        MonthlyBudget[] budgets = [new(2026, 9, 1000), new(2026, 9, 600, Guid.NewGuid())];
        var error = BudgetAllocationValidator.Validate(budgets, 2026, 9, amount, Guid.NewGuid(), null);
        Assert.Equal(allowed, error is null);
    }

    [Fact]
    public void Updating_category_replaces_old_amount()
    {
        var category = new MonthlyBudget(2026, 9, 600, Guid.NewGuid());
        MonthlyBudget[] budgets = [new(2026, 9, 1000), category];
        Assert.Null(BudgetAllocationValidator.Validate(budgets, 2026, 9, 1000, category.CategoryId, category.Id));
        Assert.NotNull(BudgetAllocationValidator.Validate(budgets, 2026, 9, 1001, category.CategoryId, category.Id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Creating_or_lowering_total_checks_all_categories(bool updating)
    {
        var total = new MonthlyBudget(2026, 9, 1000);
        List<MonthlyBudget> budgets = [new(2026, 9, 600, Guid.NewGuid()), new(2026, 9, 200, Guid.NewGuid())];
        if (updating) budgets.Add(total);
        Guid? id = updating ? total.Id : null;
        Assert.NotNull(BudgetAllocationValidator.Validate(budgets, 2026, 9, 799, null, id));
        Assert.Null(BudgetAllocationValidator.Validate(budgets, 2026, 9, 800, null, id));
    }

    [Fact]
    public void Categories_are_unrestricted_without_total_and_other_months_are_ignored()
    {
        MonthlyBudget[] budgets = [new(2026, 8, 100), new(2026, 9, 600, Guid.NewGuid())];
        Assert.Null(BudgetAllocationValidator.Validate(budgets, 2026, 9, 1000, Guid.NewGuid(), null));
    }

    [Fact]
    public void Moving_category_checks_destination_month()
    {
        var category = new MonthlyBudget(2026, 8, 600, Guid.NewGuid());
        MonthlyBudget[] budgets = [category, new(2026, 9, 500)];
        Assert.NotNull(BudgetAllocationValidator.Validate(budgets, 2026, 9, 600, category.CategoryId, category.Id));
    }

    [Fact]
    public void Converting_category_to_total_excludes_its_previous_allocation()
    {
        var category = new MonthlyBudget(2026, 9, 600, Guid.NewGuid());
        MonthlyBudget[] budgets = [category, new(2026, 9, 200, Guid.NewGuid())];
        Assert.Null(BudgetAllocationValidator.Validate(budgets, 2026, 9, 200, null, category.Id));
        Assert.NotNull(BudgetAllocationValidator.Validate(budgets, 2026, 9, 199, null, category.Id));
    }
}
