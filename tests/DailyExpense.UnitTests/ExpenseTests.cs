using DailyExpense.Domain.Expenses;

namespace DailyExpense.UnitTests;

public sealed class ExpenseTests
{
    [Fact]
    public void Constructor_requires_positive_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Expense("Coffee", 0, new DateOnly(2026, 8, 7), Guid.NewGuid()));
    }

    [Fact]
    public void Constructor_requires_category()
    {
        Assert.Throws<ArgumentException>(() =>
            new Expense("Coffee", 4.50m, new DateOnly(2026, 8, 7), Guid.Empty));
    }

    [Fact]
    public void Update_marks_expense_as_updated()
    {
        var expense = new Expense("Coffee", 4.50m, new DateOnly(2026, 8, 7), Guid.NewGuid());

        expense.Update("Lunch", 12.75m, new DateOnly(2026, 8, 8), Guid.NewGuid(), PaymentMethod.CreditCard, null);

        Assert.Equal("Lunch", expense.Title);
        Assert.NotNull(expense.UpdatedAt);
    }
}
