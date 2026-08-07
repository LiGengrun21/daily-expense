using DailyExpense.Api.Endpoints;
using DailyExpense.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? [];

        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Daily Expense Manager API",
        Version = "v1",
        Description = "Personal daily expense management API."
    });
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id
            ?? context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        var statusCode = exception is BadHttpRequestException
            ? StatusCodes.Status400BadRequest
            : StatusCodes.Status500InternalServerError;
        var title = statusCode == StatusCodes.Status400BadRequest
            ? "Invalid request"
            : "Unexpected server error";
        var extensions = new Dictionary<string, object?>
        {
            ["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier
        };

        await Results.Problem(
            title: title,
            detail: app.Environment.IsDevelopment()
                ? exception?.Message
                : statusCode == StatusCodes.Status400BadRequest
                    ? "The request is invalid."
                    : "An unexpected error occurred.",
            statusCode: statusCode,
            instance: context.Request.Path,
            extensions: extensions)
            .ExecuteAsync(context);
    });
});

app.UseStatusCodePages(async statusCodeContext =>
{
    var httpContext = statusCodeContext.HttpContext;

    if (httpContext.Response.HasStarted
        || httpContext.Response.ContentType is not null
        || httpContext.Response.StatusCode < 400)
    {
        return;
    }

    var extensions = new Dictionary<string, object?>
    {
        ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier
    };

    await Results.Problem(
        title: ReasonPhrases.GetReasonPhrase(httpContext.Response.StatusCode),
        statusCode: httpContext.Response.StatusCode,
        instance: httpContext.Request.Path,
        extensions: extensions)
        .ExecuteAsync(httpContext);
});

app.UseCors("BlazorClient");

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Daily Expense Manager API v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Ok(new
{
    Application = "Daily Expense Manager API",
    Version = "v1"
}));

app.MapHealthChecks("/health");
app.MapBudgetEndpoints();
app.MapCategoryEndpoints();
app.MapExpenseEndpoints();
app.MapReportEndpoints();

app.Run();

public partial class Program;
