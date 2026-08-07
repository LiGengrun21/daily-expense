using DailyExpense.Application.Budgets;
using DailyExpense.Contracts.Budgets;
using DailyExpense.Domain.Budgets;
using DailyExpense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyExpense.Infrastructure.Budgets;

public sealed class MonthlyBudgetService(DailyExpenseDbContext dbContext) : IMonthlyBudgetService
{
    public async Task<IReadOnlyList<MonthlyBudgetResponse>> GetBudgetsAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var periodError = ValidatePeriod(year, month);
        if (periodError is not null)
        {
            return [];
        }

        var budgets = await dbContext.MonthlyBudgets
            .AsNoTracking()
            .Where(budget => budget.Year == year && budget.Month == month)
            .OrderBy(budget => budget.CategoryId == null ? 0 : 1)
            .ThenBy(budget => budget.CategoryId)
            .ToListAsync(cancellationToken);

        return await BuildResponsesAsync(budgets, cancellationToken);
    }

    public async Task<MonthlyBudgetResponse?> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var budget = await dbContext.MonthlyBudgets
            .AsNoTracking()
            .SingleOrDefaultAsync(budget => budget.Id == id, cancellationToken);

        if (budget is null)
        {
            return null;
        }

        return (await BuildResponsesAsync([budget], cancellationToken)).Single();
    }

    public async Task<BudgetServiceResult<MonthlyBudgetResponse>> CreateBudgetAsync(
        CreateMonthlyBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateBudgetInputAsync(
            request.Year,
            request.Month,
            request.Amount,
            request.CategoryId,
            existingBudgetId: null,
            cancellationToken);

        if (validationError is not null)
        {
            return BudgetServiceResult<MonthlyBudgetResponse>.Failure(validationError.Value.Code, validationError.Value.Message);
        }

        var budget = new MonthlyBudget(
            request.Year,
            request.Month,
            request.Amount,
            request.CategoryId);

        dbContext.MonthlyBudgets.Add(budget);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await GetBudgetByIdAsync(budget.Id, cancellationToken);
        return BudgetServiceResult<MonthlyBudgetResponse>.Success(response!);
    }

    public async Task<BudgetServiceResult<MonthlyBudgetResponse>> UpdateBudgetAsync(
        Guid id,
        UpdateMonthlyBudgetRequest request,
        CancellationToken cancellationToken)
    {
        var budget = await dbContext.MonthlyBudgets.SingleOrDefaultAsync(
            budget => budget.Id == id,
            cancellationToken);

        if (budget is null)
        {
            return BudgetServiceResult<MonthlyBudgetResponse>.Failure("Budget.NotFound", "Monthly budget was not found.");
        }

        var validationError = await ValidateBudgetInputAsync(
            request.Year,
            request.Month,
            request.Amount,
            request.CategoryId,
            existingBudgetId: id,
            cancellationToken);

        if (validationError is not null)
        {
            return BudgetServiceResult<MonthlyBudgetResponse>.Failure(validationError.Value.Code, validationError.Value.Message);
        }

        budget.Update(request.Year, request.Month, request.Amount, request.CategoryId);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await GetBudgetByIdAsync(id, cancellationToken);
        return BudgetServiceResult<MonthlyBudgetResponse>.Success(response!);
    }

    public async Task<BudgetServiceResult<bool>> DeleteBudgetAsync(Guid id, CancellationToken cancellationToken)
    {
        var budget = await dbContext.MonthlyBudgets.SingleOrDefaultAsync(
            budget => budget.Id == id,
            cancellationToken);

        if (budget is null)
        {
            return BudgetServiceResult<bool>.Failure("Budget.NotFound", "Monthly budget was not found.");
        }

        dbContext.MonthlyBudgets.Remove(budget);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BudgetServiceResult<bool>.Success(true);
    }

    private async Task<IReadOnlyList<MonthlyBudgetResponse>> BuildResponsesAsync(
        IReadOnlyList<MonthlyBudget> budgets,
        CancellationToken cancellationToken)
    {
        if (budgets.Count == 0)
        {
            return [];
        }

        var year = budgets[0].Year;
        var month = budgets[0].Month;
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var categoryIds = budgets
            .Where(budget => budget.CategoryId is not null)
            .Select(budget => budget.CategoryId!.Value)
            .Distinct()
            .ToArray();

        var categoryNames = await dbContext.Categories
            .AsNoTracking()
            .Where(category => categoryIds.Contains(category.Id))
            .ToDictionaryAsync(category => category.Id, category => category.Name, cancellationToken);

        var totalSpent = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.ExpenseDate >= startDate && expense.ExpenseDate <= endDate)
            .SumAsync(expense => (decimal?)expense.Amount, cancellationToken) ?? 0m;

        var spentByCategory = await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.ExpenseDate >= startDate
                && expense.ExpenseDate <= endDate
                && categoryIds.Contains(expense.CategoryId))
            .GroupBy(expense => expense.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                SpentAmount = group.Sum(expense => expense.Amount)
            })
            .ToDictionaryAsync(item => item.CategoryId, item => item.SpentAmount, cancellationToken);

        return budgets
            .Select(budget =>
            {
                var spentAmount = budget.CategoryId is null
                    ? totalSpent
                    : spentByCategory.GetValueOrDefault(budget.CategoryId.Value);
                var remainingAmount = budget.Amount - spentAmount;
                var usagePercentage = budget.Amount == 0
                    ? 0
                    : Math.Round(spentAmount / budget.Amount * 100, 2);

                return new MonthlyBudgetResponse(
                    budget.Id,
                    budget.Year,
                    budget.Month,
                    budget.Amount,
                    budget.CategoryId,
                    budget.CategoryId is null ? null : categoryNames.GetValueOrDefault(budget.CategoryId.Value),
                    spentAmount,
                    remainingAmount,
                    usagePercentage,
                    spentAmount > budget.Amount,
                    budget.CreatedAt,
                    budget.UpdatedAt);
            })
            .ToList();
    }

    private async Task<(string Code, string Message)?> ValidateBudgetInputAsync(
        int year,
        int month,
        decimal amount,
        Guid? categoryId,
        Guid? existingBudgetId,
        CancellationToken cancellationToken)
    {
        var periodError = ValidatePeriod(year, month);
        if (periodError is not null)
        {
            return periodError;
        }

        if (amount <= 0)
        {
            return ("Budget.AmountInvalid", "Budget amount must be greater than zero.");
        }

        if (categoryId == Guid.Empty)
        {
            return ("Budget.CategoryInvalid", "Budget category is invalid.");
        }

        if (categoryId is not null)
        {
            var categoryExists = await dbContext.Categories.AnyAsync(
                category => category.Id == categoryId.Value,
                cancellationToken);

            if (!categoryExists)
            {
                return ("Category.NotFound", "Budget category was not found.");
            }
        }

        var duplicateExists = await dbContext.MonthlyBudgets.AnyAsync(
            budget => budget.Year == year
                && budget.Month == month
                && budget.CategoryId == categoryId
                && (existingBudgetId == null || budget.Id != existingBudgetId.Value),
            cancellationToken);

        if (duplicateExists)
        {
            return categoryId is null
                ? ("Budget.TotalAlreadyExists", "Total monthly budget already exists for this period.")
                : ("Budget.CategoryAlreadyExists", "Category monthly budget already exists for this period.");
        }

        return null;
    }

    private static (string Code, string Message)? ValidatePeriod(int year, int month)
    {
        if (year < 2000 || year > 2100)
        {
            return ("Budget.YearInvalid", "Budget year must be between 2000 and 2100.");
        }

        if (month is < 1 or > 12)
        {
            return ("Budget.MonthInvalid", "Budget month must be between 1 and 12.");
        }

        return null;
    }
}
