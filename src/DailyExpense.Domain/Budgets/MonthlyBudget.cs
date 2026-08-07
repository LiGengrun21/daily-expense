namespace DailyExpense.Domain.Budgets;

public sealed class MonthlyBudget
{
    private MonthlyBudget()
    {
    }

    public MonthlyBudget(int year, int month, decimal amount, Guid? categoryId = null)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;

        SetPeriod(year, month);
        SetAmount(amount);
        CategoryId = categoryId;
    }

    public Guid Id { get; private set; }

    public int Year { get; private set; }

    public int Month { get; private set; }

    public decimal Amount { get; private set; }

    public Guid? CategoryId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(int year, int month, decimal amount, Guid? categoryId)
    {
        SetPeriod(year, month);
        SetAmount(amount);
        CategoryId = categoryId;
        MarkUpdated();
    }

    private void SetPeriod(int year, int month)
    {
        if (year < 2000 || year > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Budget year must be between 2000 and 2100.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Budget month must be between 1 and 12.");
        }

        Year = year;
        Month = month;
    }

    private void SetAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Budget amount must be greater than zero.");
        }

        Amount = amount;
    }

    private void MarkUpdated()
    {
        if (CreatedAt != default)
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
