using DailyExpense.Application.Budgets;
using DailyExpense.Contracts.Budgets;

namespace DailyExpense.Api.Endpoints;

public static class BudgetEndpoints
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/budgets")
            .WithTags("Budgets");

        group.MapGet("/", async (
            IMonthlyBudgetService budgetService,
            int year,
            int month,
            CancellationToken cancellationToken) =>
        {
            var budgets = await budgetService.GetBudgetsAsync(year, month, cancellationToken);
            return Results.Ok(budgets);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IMonthlyBudgetService budgetService,
            CancellationToken cancellationToken) =>
        {
            var budget = await budgetService.GetBudgetByIdAsync(id, cancellationToken);
            return budget is null ? Results.NotFound() : Results.Ok(budget);
        });

        group.MapPost("/", async (
            CreateMonthlyBudgetRequest request,
            IMonthlyBudgetService budgetService,
            CancellationToken cancellationToken) =>
        {
            var result = await budgetService.CreateBudgetAsync(request, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.Created($"/api/v1/budgets/{result.Value!.Id}", result.Value);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateMonthlyBudgetRequest request,
            IMonthlyBudgetService budgetService,
            CancellationToken cancellationToken) =>
        {
            var result = await budgetService.UpdateBudgetAsync(id, request, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.Ok(result.Value);
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMonthlyBudgetService budgetService,
            CancellationToken cancellationToken) =>
        {
            var result = await budgetService.DeleteBudgetAsync(id, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.NoContent();
        });

        return endpoints;
    }

    private static IResult ToProblemResult<T>(BudgetServiceResult<T> result)
    {
        var statusCode = result.ErrorCode switch
        {
            "Budget.NotFound" => StatusCodes.Status404NotFound,
            "Budget.TotalAlreadyExists" => StatusCodes.Status409Conflict,
            "Budget.CategoryAlreadyExists" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            title: result.ErrorCode,
            detail: result.ErrorMessage,
            statusCode: statusCode);
    }
}
