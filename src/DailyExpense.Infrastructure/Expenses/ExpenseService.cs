using DailyExpense.Application.Expenses;
using DailyExpense.Contracts.Common;
using DailyExpense.Contracts.Expenses;
using DailyExpense.Domain.Expenses;
using DailyExpense.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DailyExpense.Infrastructure.Expenses;

public sealed class ExpenseService(DailyExpenseDbContext dbContext) : IExpenseService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResponse<ExpenseResponse>> GetExpensesAsync(
        DateOnly? from,
        DateOnly? to,
        Guid? categoryId,
        decimal? minAmount,
        decimal? maxAmount,
        int page,
        int pageSize,
        string? sort,
        CancellationToken cancellationToken)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = dbContext.Expenses.AsNoTracking();

        if (from is not null)
        {
            query = query.Where(expense => expense.ExpenseDate >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(expense => expense.ExpenseDate <= to.Value);
        }

        if (categoryId is not null)
        {
            query = query.Where(expense => expense.CategoryId == categoryId.Value);
        }

        if (minAmount is not null)
        {
            query = query.Where(expense => expense.Amount >= minAmount.Value);
        }

        if (maxAmount is not null)
        {
            query = query.Where(expense => expense.Amount <= maxAmount.Value);
        }

        query = ApplySorting(query, sort);

        var totalCount = await query.CountAsync(cancellationToken);
        var skip = (page - 1) * pageSize;

        var items = await (
            from expense in query.Skip(skip).Take(pageSize)
            join category in dbContext.Categories.AsNoTracking()
                on expense.CategoryId equals category.Id
            select new ExpenseResponse(
                expense.Id,
                expense.Title,
                expense.Description,
                expense.Amount,
                expense.ExpenseDate,
                expense.CategoryId,
                category.Name,
                expense.PaymentMethod.ToString(),
                expense.CreatedAt,
                expense.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResponse<ExpenseResponse>(items, page, pageSize, totalCount);
    }

    public async Task<ExpenseResponse?> GetExpenseByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await (
            from expense in dbContext.Expenses.AsNoTracking()
            join category in dbContext.Categories.AsNoTracking()
                on expense.CategoryId equals category.Id
            where expense.Id == id
            select new ExpenseResponse(
                expense.Id,
                expense.Title,
                expense.Description,
                expense.Amount,
                expense.ExpenseDate,
                expense.CategoryId,
                category.Name,
                expense.PaymentMethod.ToString(),
                expense.CreatedAt,
                expense.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<ExpenseServiceResult<ExpenseResponse>> CreateExpenseAsync(
        CreateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = await ValidateExpenseInputAsync(
            request.Title,
            request.Amount,
            request.CategoryId,
            request.PaymentMethod,
            cancellationToken);

        if (validationError is not null)
        {
            return ExpenseServiceResult<ExpenseResponse>.Failure(validationError.Value.Code, validationError.Value.Message);
        }

        var expense = new Expense(
            request.Title,
            request.Amount,
            request.ExpenseDate,
            request.CategoryId,
            ParsePaymentMethod(request.PaymentMethod),
            request.Description);

        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await GetExpenseByIdAsync(expense.Id, cancellationToken);
        return ExpenseServiceResult<ExpenseResponse>.Success(response!);
    }

    public async Task<ExpenseServiceResult<ExpenseResponse>> UpdateExpenseAsync(
        Guid id,
        UpdateExpenseRequest request,
        CancellationToken cancellationToken)
    {
        var expense = await dbContext.Expenses.SingleOrDefaultAsync(
            expense => expense.Id == id,
            cancellationToken);

        if (expense is null)
        {
            return ExpenseServiceResult<ExpenseResponse>.Failure("Expense.NotFound", "Expense was not found.");
        }

        var validationError = await ValidateExpenseInputAsync(
            request.Title,
            request.Amount,
            request.CategoryId,
            request.PaymentMethod,
            cancellationToken);

        if (validationError is not null)
        {
            return ExpenseServiceResult<ExpenseResponse>.Failure(validationError.Value.Code, validationError.Value.Message);
        }

        expense.Update(
            request.Title,
            request.Amount,
            request.ExpenseDate,
            request.CategoryId,
            ParsePaymentMethod(request.PaymentMethod),
            request.Description);

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = await GetExpenseByIdAsync(id, cancellationToken);
        return ExpenseServiceResult<ExpenseResponse>.Success(response!);
    }

    public async Task<ExpenseServiceResult<bool>> DeleteExpenseAsync(Guid id, CancellationToken cancellationToken)
    {
        var expense = await dbContext.Expenses.SingleOrDefaultAsync(
            expense => expense.Id == id,
            cancellationToken);

        if (expense is null)
        {
            return ExpenseServiceResult<bool>.Failure("Expense.NotFound", "Expense was not found.");
        }

        dbContext.Expenses.Remove(expense);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseServiceResult<bool>.Success(true);
    }

    private static IQueryable<Expense> ApplySorting(IQueryable<Expense> query, string? sort)
    {
        return sort?.Trim().ToLowerInvariant() switch
        {
            "date_asc" => query.OrderBy(expense => expense.ExpenseDate).ThenBy(expense => expense.CreatedAt),
            "amount_asc" => query.OrderBy(expense => expense.Amount).ThenByDescending(expense => expense.ExpenseDate),
            "amount_desc" => query.OrderByDescending(expense => expense.Amount).ThenByDescending(expense => expense.ExpenseDate),
            "created_asc" => query.OrderBy(expense => expense.CreatedAt),
            "created_desc" => query.OrderByDescending(expense => expense.CreatedAt),
            _ => query.OrderByDescending(expense => expense.ExpenseDate).ThenByDescending(expense => expense.CreatedAt)
        };
    }

    private async Task<(string Code, string Message)?> ValidateExpenseInputAsync(
        string title,
        decimal amount,
        Guid categoryId,
        string? paymentMethod,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return ("Expense.TitleRequired", "Expense title is required.");
        }

        if (amount <= 0)
        {
            return ("Expense.AmountInvalid", "Expense amount must be greater than zero.");
        }

        if (categoryId == Guid.Empty)
        {
            return ("Expense.CategoryRequired", "Expense category is required.");
        }

        if (!string.IsNullOrWhiteSpace(paymentMethod)
            && !Enum.TryParse<PaymentMethod>(paymentMethod, ignoreCase: true, out _))
        {
            return ("Expense.PaymentMethodInvalid", "Expense payment method is invalid.");
        }

        var categoryExists = await dbContext.Categories.AnyAsync(
            category => category.Id == categoryId,
            cancellationToken);

        if (!categoryExists)
        {
            return ("Category.NotFound", "Expense category was not found.");
        }

        return null;
    }

    private static PaymentMethod ParsePaymentMethod(string? paymentMethod)
    {
        return Enum.TryParse<PaymentMethod>(paymentMethod, ignoreCase: true, out var parsed)
            ? parsed
            : PaymentMethod.Unknown;
    }
}
