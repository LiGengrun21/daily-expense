using DailyExpense.Application.Expenses;
using DailyExpense.Contracts.Expenses;

namespace DailyExpense.Api.Endpoints;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/expenses")
            .WithTags("Expenses");

        group.MapGet("/", async (
            IExpenseService expenseService,
            DateOnly? from,
            DateOnly? to,
            Guid? categoryId,
            decimal? minAmount,
            decimal? maxAmount,
            int? page,
            int? pageSize,
            string? sort,
            CancellationToken cancellationToken) =>
        {
            var result = await expenseService.GetExpensesAsync(
                from,
                to,
                categoryId,
                minAmount,
                maxAmount,
                page.GetValueOrDefault(1),
                pageSize.GetValueOrDefault(20),
                sort,
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IExpenseService expenseService,
            CancellationToken cancellationToken) =>
        {
            var expense = await expenseService.GetExpenseByIdAsync(id, cancellationToken);
            return expense is null ? Results.NotFound() : Results.Ok(expense);
        });

        group.MapPost("/", async (
            CreateExpenseRequest request,
            IExpenseService expenseService,
            CancellationToken cancellationToken) =>
        {
            var result = await expenseService.CreateExpenseAsync(request, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.Created($"/api/v1/expenses/{result.Value!.Id}", result.Value);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateExpenseRequest request,
            IExpenseService expenseService,
            CancellationToken cancellationToken) =>
        {
            var result = await expenseService.UpdateExpenseAsync(id, request, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.Ok(result.Value);
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            IExpenseService expenseService,
            CancellationToken cancellationToken) =>
        {
            var result = await expenseService.DeleteExpenseAsync(id, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.NoContent();
        });

        return endpoints;
    }

    private static IResult ToProblemResult<T>(ExpenseServiceResult<T> result)
    {
        var statusCode = result.ErrorCode switch
        {
            "Expense.NotFound" => StatusCodes.Status404NotFound,
            "Category.NotFound" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            title: result.ErrorCode,
            detail: result.ErrorMessage,
            statusCode: statusCode);
    }
}
