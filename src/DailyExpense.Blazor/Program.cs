using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DailyExpense.Blazor;
using DailyExpense.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiOptions = builder.Configuration.GetSection("Api").Get<ApiOptions>() ?? new ApiOptions();
builder.Services.AddSingleton(apiOptions);
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiOptions.BaseUrl) });
builder.Services.AddScoped<DailyExpenseApiClient>();

await builder.Build().RunAsync();
