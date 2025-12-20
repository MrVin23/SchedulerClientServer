using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Interfaces;
using Client.Interfaces.Authorisation;
using Client.Services.Authorisation;
using Client.Services;
using Client.Services.HttpServices;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient for cookie authentication
// In Blazor WebAssembly, cookies need credentials to be included for cross-origin requests
// We'll use a custom HttpClient that automatically includes credentials
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient
    {
        // Point to the API server URL
        // TODO: Make this configurable via appsettings.json for different environments
        BaseAddress = new Uri("http://localhost:5097")
    };
    
    return httpClient;
});

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISecureStorageService, SecureStorageService>();
builder.Services.AddScoped<IUserInfoService, UserInfoService>();
builder.Services.AddScoped<ITokenServices, TokenServices>();
builder.Services.AddScoped<IEventsService, EventsService>();

// Register TokenRefreshService as singleton so it can monitor token across the app
builder.Services.AddSingleton<TokenRefreshService>();

// Register AlertService as singleton so alerts persist across components
builder.Services.AddSingleton<IAlertService, AlertService>();

await builder.Build().RunAsync();
