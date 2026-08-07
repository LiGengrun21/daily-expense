using DailyExpense.Domain.Budgets;
using DailyExpense.Domain.Categories;
using DailyExpense.Domain.Expenses;
using Microsoft.EntityFrameworkCore;

namespace DailyExpense.Infrastructure.Persistence;

public sealed class DailyExpenseDbContext(DbContextOptions<DailyExpenseDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Expense> Expenses => Set<Expense>();

    public DbSet<MonthlyBudget> MonthlyBudgets => Set<MonthlyBudget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DailyExpenseDbContext).Assembly);
    }
}
