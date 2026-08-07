using DailyExpense.Domain.Categories;
using DailyExpense.Domain.Expenses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyExpense.Infrastructure.Persistence.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(expense => expense.Id);

        builder.Property(expense => expense.Id).HasColumnName("id");
        builder.Property(expense => expense.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(expense => expense.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(expense => expense.Amount).HasColumnName("amount").HasPrecision(18, 2).IsRequired();
        builder.Property(expense => expense.ExpenseDate).HasColumnName("expense_date").IsRequired();
        builder.Property(expense => expense.CategoryId).HasColumnName("category_id").IsRequired();
        builder.Property(expense => expense.PaymentMethod).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(50);
        builder.Property(expense => expense.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(expense => expense.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(expense => expense.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint("ck_expenses_amount", "amount > 0");
        });

        builder.HasIndex(expense => expense.ExpenseDate);
        builder.HasIndex(expense => expense.CategoryId);
        builder.HasIndex(expense => new { expense.CategoryId, expense.ExpenseDate });
    }
}
