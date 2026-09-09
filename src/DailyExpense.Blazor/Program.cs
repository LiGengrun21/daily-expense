using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DailyExpense.Blazor;
using DailyExpense.Blazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Local values stay outside version control; production receives container-generated config.
if (builder.HostEnvironment.IsDevelopment())
{
    using var configurationClient = new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    using var response = await configurationClient.GetAsync("appsettings.Local.json");
    // SPA development hosts can return index.html for a missing optional JSON file.
    if (response.StatusCode != System.Net.HttpStatusCode.NotFound
        && response.Content.Headers.ContentType?.MediaType != "text/html")
    {
        response.EnsureSuccessStatusCode();
        using var stream = new MemoryStream(await response.Content.ReadAsByteArrayAsync());
        builder.Configuration.AddJsonStream(stream);
    }
}

var tenantId = builder.Configuration["AzureAd:TenantId"];
var clientId = builder.Configuration["AzureAd:ClientId"];
var signInConfigured = Guid.TryParse(tenantId, out var tenantGuid) && tenantGuid != Guid.Empty
    && Guid.TryParse(clientId, out var parsedClientId) && parsedClientId != Guid.Empty;
builder.Services.AddSingleton(new SignInConfiguration(signInConfigured));

// Configuration is intentionally blank until the Entra app registration is supplied.
if (signInConfigured)
{
    builder.Services.AddMsalAuthentication(options =>
    {
        options.ProviderOptions.Authentication.Authority =
            $"https://login.microsoftonline.com/{tenantGuid:D}";
        options.ProviderOptions.Authentication.ClientId = Guid.Parse(clientId!).ToString("D");
        options.ProviderOptions.Authentication.ValidateAuthority = true;
        options.ProviderOptions.Authentication.RedirectUri =
            new Uri(new Uri(builder.HostEnvironment.BaseAddress), "authentication/login-callback").AbsoluteUri;
        options.ProviderOptions.Authentication.PostLogoutRedirectUri =
            new Uri(new Uri(builder.HostEnvironment.BaseAddress), "authentication/logout-callback").AbsoluteUri;
        options.ProviderOptions.LoginMode = "redirect";
    });
}

var apiOptions = builder.Configuration.GetSection("Api").Get<ApiOptions>() ?? new ApiOptions();
builder.Services.AddSingleton(apiOptions);
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiOptions.BaseUrl) });
builder.Services.AddScoped<DailyExpenseApiClient>();

await builder.Build().RunAsync();
