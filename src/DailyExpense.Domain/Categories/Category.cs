namespace DailyExpense.Domain.Categories;

public sealed class Category
{
    private Category()
    {
    }

    public Category(
        string name,
        string? description = null,
        string? color = null,
        string? icon = null,
        bool isDefault = false)
    {
        Id = Guid.NewGuid();

        Rename(name);
        UpdateDetails(description, color, icon);
        IsDefault = isDefault;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = null;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Color { get; private set; }

    public string? Icon { get; private set; }

    public bool IsDefault { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", nameof(name));
        }

        Name = name.Trim();
        MarkUpdated();
    }

    public void UpdateDetails(string? description, string? color, string? icon)
    {
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
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
