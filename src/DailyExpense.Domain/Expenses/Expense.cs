namespace DailyExpense.Domain.Expenses;

public sealed class Expense
{
    private Expense()
    {
    }

    public Expense(
        string title,
        decimal amount,
        DateOnly expenseDate,
        Guid categoryId,
        PaymentMethod paymentMethod = PaymentMethod.Unknown,
        string? description = null)
    {
        Id = Guid.NewGuid();

        Update(title, amount, expenseDate, categoryId, paymentMethod, description);
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = null;
    }

    public Guid Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public decimal Amount { get; private set; }

    public DateOnly ExpenseDate { get; private set; }

    public Guid CategoryId { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Update(
        string title,
        decimal amount,
        DateOnly expenseDate,
        Guid categoryId,
        PaymentMethod paymentMethod,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Expense title is required.", nameof(title));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Expense amount must be greater than zero.");
        }

        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Expense category is required.", nameof(categoryId));
        }

        Title = title.Trim();
        Amount = amount;
        ExpenseDate = expenseDate;
        CategoryId = categoryId;
        PaymentMethod = paymentMethod;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        MarkUpdated();
    }

    private void MarkUpdated()
    {
        if (CreatedAt != default)
        {
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
