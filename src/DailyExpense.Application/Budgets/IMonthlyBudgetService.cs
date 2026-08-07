using DailyExpense.Contracts.Budgets;

namespace DailyExpense.Application.Budgets;

public interface IMonthlyBudgetService
{
    Task<IReadOnlyList<MonthlyBudgetResponse>> GetBudgetsAsync(
        int year,
        int month,
        CancellationToken cancellationToken);

    Task<MonthlyBudgetResponse?> GetBudgetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<BudgetServiceResult<MonthlyBudgetResponse>> CreateBudgetAsync(
        CreateMonthlyBudgetRequest request,
        CancellationToken cancellationToken);

    Task<BudgetServiceResult<MonthlyBudgetResponse>> UpdateBudgetAsync(
        Guid id,
        UpdateMonthlyBudgetRequest request,
        CancellationToken cancellationToken);

    Task<BudgetServiceResult<bool>> DeleteBudgetAsync(Guid id, CancellationToken cancellationToken);
}
