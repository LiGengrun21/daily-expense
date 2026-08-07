using DailyExpense.Application.Budgets;
using DailyExpense.Application.Categories;
using DailyExpense.Application.Expenses;
using DailyExpense.Application.Reports;
using DailyExpense.Infrastructure.Budgets;
using DailyExpense.Infrastructure.Categories;
using DailyExpense.Infrastructure.Expenses;
using DailyExpense.Infrastructure.Persistence;
using DailyExpense.Infrastructure.Reports;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DailyExpense.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<DailyExpenseDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IMonthlyBudgetService, MonthlyBudgetService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
