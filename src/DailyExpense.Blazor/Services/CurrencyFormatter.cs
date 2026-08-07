using System.Globalization;

namespace DailyExpense.Blazor.Services;

public static class CurrencyFormatter
{
    private static readonly CultureInfo NumberCulture = CultureInfo.GetCultureInfo("de-DE");

    public static string Format(decimal amount)
    {
        return $"{amount.ToString("N2", NumberCulture)} €";
    }

    public static string Format(decimal? amount, string fallback = "Not set")
    {
        return amount is null ? fallback : Format(amount.Value);
    }
}
