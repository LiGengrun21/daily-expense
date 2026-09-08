using DailyExpense.Domain.Budgets;

namespace DailyExpense.Application.Budgets;

public static class BudgetAllocationValidator
{
    public static string? Validate(
        IEnumerable<MonthlyBudget> budgets,
        int year,
        int month,
        decimal amount,
        Guid? categoryId,
        Guid? existingBudgetId)
    {
        var otherBudgets = budgets
            .Where(budget => budget.Year == year && budget.Month == month && budget.Id != existingBudgetId)
            .ToList();
        var total = categoryId is null
            ? amount
            : otherBudgets.FirstOrDefault(budget => budget.CategoryId is null)?.Amount;
        var allocated = otherBudgets.Where(budget => budget.CategoryId is not null).Sum(budget => budget.Amount)
            + (categoryId is null ? 0m : amount);

        if (total is not null && allocated > total.Value)
        {
            return FormattableString.Invariant(
                $"Category budgets total ({allocated:F2}) cannot exceed Total Budget ({total.Value:F2}). Reduce category budgets or increase Total Budget.");
        }

        return null;
    }
}
