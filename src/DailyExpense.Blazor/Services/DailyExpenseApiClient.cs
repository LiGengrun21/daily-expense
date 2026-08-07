using System.Net.Http.Json;
using System.Text.Json;
using DailyExpense.Contracts.Budgets;
using DailyExpense.Contracts.Categories;
using DailyExpense.Contracts.Common;
using DailyExpense.Contracts.Dashboard;
using DailyExpense.Contracts.Expenses;
using DailyExpense.Contracts.Statistics;
using DailyExpense.Contracts.Summaries;

namespace DailyExpense.Blazor.Services;

public sealed class DailyExpenseApiClient(HttpClient httpClient)
{
    public async Task<DashboardResponse?> GetDashboardAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<DashboardResponse>(
            $"/api/v1/dashboard?year={year}&month={month}",
            cancellationToken);
    }

    public async Task<IReadOnlyList<CategoryResponse>> GetCategoriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyList<CategoryResponse>>(
            "/api/v1/categories",
            cancellationToken) ?? [];
    }

    public async Task<CategoryResponse?> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/v1/categories", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<CategoryResponse>(cancellationToken);
    }

    public async Task<CategoryResponse?> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/v1/categories/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<CategoryResponse>(cancellationToken);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"/api/v1/categories/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<MonthlyBudgetResponse>> GetBudgetsAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<IReadOnlyList<MonthlyBudgetResponse>>(
            $"/api/v1/budgets?year={year}&month={month}",
            cancellationToken) ?? [];
    }

    public async Task<MonthlyBudgetResponse?> CreateBudgetAsync(
        CreateMonthlyBudgetRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/v1/budgets", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<MonthlyBudgetResponse>(cancellationToken);
    }

    public async Task<MonthlyBudgetResponse?> UpdateBudgetAsync(
        Guid id,
        UpdateMonthlyBudgetRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/v1/budgets/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<MonthlyBudgetResponse>(cancellationToken);
    }

    public async Task DeleteBudgetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"/api/v1/budgets/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<MonthlySummaryResponse?> GetMonthlySummaryAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<MonthlySummaryResponse>(
            $"/api/v1/summaries/monthly?year={year}&month={month}",
            cancellationToken);
    }

    public async Task<YearlySummaryResponse?> GetYearlySummaryAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<YearlySummaryResponse>(
            $"/api/v1/summaries/yearly?year={year}",
            cancellationToken);
    }

    public async Task<CategoryStatisticsResponse?> GetCategoryStatisticsAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<CategoryStatisticsResponse>(
            $"/api/v1/statistics/by-category?from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}",
            cancellationToken);
    }

    public async Task<DailyStatisticsResponse?> GetDailyStatisticsAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<DailyStatisticsResponse>(
            $"/api/v1/statistics/by-day?year={year}&month={month}",
            cancellationToken);
    }

    public async Task<TrendStatisticsResponse?> GetTrendStatisticsAsync(
        int months,
        CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TrendStatisticsResponse>(
            $"/api/v1/statistics/trends?months={months}",
            cancellationToken);
    }

    public async Task<PagedResponse<ExpenseResponse>?> GetExpensesAsync(
        DateOnly? from,
        DateOnly? to,
        Guid? categoryId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"page={page}",
            $"pageSize={pageSize}",
            "sort=date_desc"
        };

        if (from is not null)
        {
            query.Add($"from={from:yyyy-MM-dd}");
        }

        if (to is not null)
        {
            query.Add($"to={to:yyyy-MM-dd}");
        }

        if (categoryId is not null)
        {
            query.Add($"categoryId={categoryId}");
        }

        return await httpClient.GetFromJsonAsync<PagedResponse<ExpenseResponse>>(
            $"/api/v1/expenses?{string.Join("&", query)}",
            cancellationToken);
    }

    public async Task<ExpenseResponse?> CreateExpenseAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("/api/v1/expenses", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<ExpenseResponse>(cancellationToken);
    }

    public async Task<ExpenseResponse?> UpdateExpenseAsync(
        Guid id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"/api/v1/expenses/{id}", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<ExpenseResponse>(cancellationToken);
    }

    public async Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"/api/v1/expenses/{id}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await ReadErrorDetailAsync(response, cancellationToken);
        throw new HttpRequestException(
            string.IsNullOrWhiteSpace(detail)
                ? $"API request failed with status {(int)response.StatusCode}."
                : detail);
    }

    private static async Task<string?> ReadErrorDetailAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("detail", out var detailElement))
            {
                return detailElement.GetString();
            }

            if (document.RootElement.TryGetProperty("title", out var titleElement))
            {
                return titleElement.GetString();
            }
        }
        catch (JsonException)
        {
            return content;
        }

        return content;
    }
}
