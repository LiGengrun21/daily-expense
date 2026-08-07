namespace DailyExpense.Infrastructure.Persistence.Seed;

internal static class DefaultCategories
{
    private static readonly DateTimeOffset SeededAt = new(2026, 8, 7, 0, 0, 0, TimeSpan.Zero);

    public static readonly object[] All =
    [
        Create("11111111-1111-1111-1111-111111111111", "Food", "Food and groceries", "#2F855A", "utensils"),
        Create("22222222-2222-2222-2222-222222222222", "Transport", "Public transport, fuel, rides", "#2B6CB0", "car"),
        Create("33333333-3333-3333-3333-333333333333", "Housing", "Rent, utilities, home costs", "#805AD5", "home"),
        Create("44444444-4444-4444-4444-444444444444", "Shopping", "Clothes, household items, online shopping", "#D69E2E", "shopping-bag"),
        Create("55555555-5555-5555-5555-555555555555", "Entertainment", "Movies, games, subscriptions", "#C53030", "ticket"),
        Create("66666666-6666-6666-6666-666666666666", "Health", "Medical, pharmacy, fitness", "#319795", "heart-pulse"),
        Create("77777777-7777-7777-7777-777777777777", "Education", "Books, courses, learning", "#4C51BF", "graduation-cap"),
        Create("88888888-8888-8888-8888-888888888888", "Other", "Uncategorized expenses", "#718096", "circle-help")
    ];

    private static object Create(string id, string name, string description, string color, string icon)
    {
        return new
        {
            Id = Guid.Parse(id),
            Name = name,
            Description = description,
            Color = color,
            Icon = icon,
            IsDefault = true,
            CreatedAt = SeededAt,
            UpdatedAt = (DateTimeOffset?)null
        };
    }
}
