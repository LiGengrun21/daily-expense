using DailyExpense.Domain.Budgets;
using DailyExpense.Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyExpense.Infrastructure.Persistence.Configurations;

public sealed class MonthlyBudgetConfiguration : IEntityTypeConfiguration<MonthlyBudget>
{
    public void Configure(EntityTypeBuilder<MonthlyBudget> builder)
    {
        builder.ToTable("monthly_budgets");

        builder.HasKey(budget => budget.Id);

        builder.Property(budget => budget.Id).HasColumnName("id");
        builder.Property(budget => budget.Year).HasColumnName("year").IsRequired();
        builder.Property(budget => budget.Month).HasColumnName("month").IsRequired();
        builder.Property(budget => budget.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(budget => budget.CategoryId).HasColumnName("category_id");
        builder.Property(budget => budget.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(budget => budget.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(budget => budget.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_monthly_budgets_month", "\"month\" >= 1 AND \"month\" <= 12");
            table.HasCheckConstraint("ck_monthly_budgets_amount", "amount > 0");
        });

        builder.HasIndex(budget => new { budget.Year, budget.Month, budget.CategoryId })
            .IsUnique()
            .HasFilter("category_id IS NOT NULL");

        builder.HasIndex(budget => new { budget.Year, budget.Month })
            .IsUnique()
            .HasFilter("category_id IS NULL");
    }
}
