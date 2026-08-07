namespace DailyExpense.Application.Budgets;

public sealed record BudgetServiceResult<T>(
    bool Succeeded,
    T? Value,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static BudgetServiceResult<T> Success(T value)
    {
        return new BudgetServiceResult<T>(true, value, null, null);
    }

    public static BudgetServiceResult<T> Failure(string errorCode, string errorMessage)
    {
        return new BudgetServiceResult<T>(false, default, errorCode, errorMessage);
    }
}
