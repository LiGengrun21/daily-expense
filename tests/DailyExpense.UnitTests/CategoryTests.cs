using DailyExpense.Domain.Categories;

namespace DailyExpense.UnitTests;

public sealed class CategoryTests
{
    [Fact]
    public void Constructor_requires_name()
    {
        Assert.Throws<ArgumentException>(() => new Category(""));
    }

    [Fact]
    public void Constructor_trims_name()
    {
        var category = new Category("  Food  ");

        Assert.Equal("Food", category.Name);
        Assert.Null(category.UpdatedAt);
    }
}
