using DailyExpense.Domain.Categories;
using DailyExpense.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DailyExpense.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Id).HasColumnName("id");
        builder.Property(category => category.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(category => category.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(category => category.Color).HasColumnName("color").HasMaxLength(20);
        builder.Property(category => category.Icon).HasColumnName("icon").HasMaxLength(100);
        builder.Property(category => category.IsDefault).HasColumnName("is_default").IsRequired();
        builder.Property(category => category.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(category => category.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(category => category.Name).IsUnique();

        builder.HasData(DefaultCategories.All);
    }
}
