using DailyExpense.Contracts.Common;
using DailyExpense.Contracts.Expenses;

namespace DailyExpense.Application.Expenses;

public interface IExpenseService
{
    Task<PagedResponse<ExpenseResponse>> GetExpensesAsync(
        DateOnly? from,
        DateOnly? to,
        Guid? categoryId,
        decimal? minAmount,
        decimal? maxAmount,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken);

    Task<ExpenseResponse?> GetExpenseByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<ExpenseServiceResult<ExpenseResponse>> CreateExpenseAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken);

    Task<ExpenseServiceResult<ExpenseResponse>> UpdateExpenseAsync(
        Guid id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken);

    Task<ExpenseServiceResult<bool>> DeleteExpenseAsync(Guid id, CancellationToken cancellationToken);
}
