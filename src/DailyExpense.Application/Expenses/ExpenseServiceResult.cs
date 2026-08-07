namespace DailyExpense.Application.Expenses;

public sealed record ExpenseServiceResult<T>(
    bool Succeeded,
    T? Value,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static ExpenseServiceResult<T> Success(T value)
    {
        return new ExpenseServiceResult<T>(true, value, null, null);
    }

    public static ExpenseServiceResult<T> Failure(string errorCode, string errorMessage)
    {
        return new ExpenseServiceResult<T>(false, default, errorCode, errorMessage);
    }
}
