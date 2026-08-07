using DailyExpense.Application.Categories;
using DailyExpense.Contracts.Categories;

namespace DailyExpense.Api.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/categories")
            .WithTags("Categories");

        group.MapGet("/", async (
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            var categories = await categoryService.GetCategoriesAsync(cancellationToken);
            return Results.Ok(categories);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            var category = await categoryService.GetCategoryByIdAsync(id, cancellationToken);
            return category is null ? Results.NotFound() : Results.Ok(category);
        });

        group.MapPost("/", async (
            CreateCategoryRequest request,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            var result = await categoryService.CreateCategoryAsync(request, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.Created($"/api/v1/categories/{result.Value!.Id}", result.Value);
        });

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateCategoryRequest request,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            var result = await categoryService.UpdateCategoryAsync(id, request, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.Ok(result.Value);
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            ICategoryService categoryService,
            CancellationToken cancellationToken) =>
        {
            var result = await categoryService.DeleteCategoryAsync(id, cancellationToken);

            if (!result.Succeeded)
            {
                return ToProblemResult(result);
            }

            return Results.NoContent();
        });

        return endpoints;
    }

    private static IResult ToProblemResult<T>(CategoryServiceResult<T> result)
    {
        var statusCode = result.ErrorCode switch
        {
            "Category.NotFound" => StatusCodes.Status404NotFound,
            "Category.InUse" => StatusCodes.Status409Conflict,
            "Category.DefaultCannotBeDeleted" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        return Results.Problem(
            title: result.ErrorCode,
            detail: result.ErrorMessage,
            statusCode: statusCode);
    }
}
