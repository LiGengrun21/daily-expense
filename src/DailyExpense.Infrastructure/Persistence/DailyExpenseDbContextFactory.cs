using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DailyExpense.Infrastructure.Persistence;

public sealed class DailyExpenseDbContextFactory : IDesignTimeDbContextFactory<DailyExpenseDbContext>
{
    public DailyExpenseDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=daily_expense;Username=daily_expense;Password=daily_expense";

        var optionsBuilder = new DbContextOptionsBuilder<DailyExpenseDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DailyExpenseDbContext(optionsBuilder.Options);
    }
}
