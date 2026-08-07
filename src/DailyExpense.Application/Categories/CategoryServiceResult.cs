namespace DailyExpense.Application.Categories;

public sealed record CategoryServiceResult<T>(
    bool Succeeded,
    T? Value,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static CategoryServiceResult<T> Success(T value)
    {
        return new CategoryServiceResult<T>(true, value, null, null);
    }

    public static CategoryServiceResult<T> Failure(string errorCode, string errorMessage)
    {
        return new CategoryServiceResult<T>(false, default, errorCode, errorMessage);
    }
}
