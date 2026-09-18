using CEP.FrontEnd.Auth;
using CEP.FrontEnd.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

namespace CEP.FrontEnd;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Configure Logging
        builder.Logging.AddFilter("Microsoft.AspNetCore.Authorization", LogLevel.Warning);

        // Register MudBlazor services
        builder.Services.AddMudServices();

        // Register Authorization Core
        builder.Services.AddAuthorizationCore();

        // Register Token and Auth State
        builder.Services.AddScoped<TokenService>();
        builder.Services.AddScoped<CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
            sp.GetRequiredService<CustomAuthenticationStateProvider>());

        // Register HTTP Message Handler for Bearer Token
        builder.Services.AddTransient<AuthorizationMessageHandler>();

        // Configure Backend API HttpClient
        var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5106/";
        if (!apiBaseUrl.EndsWith("/"))
        {
            apiBaseUrl += "/";
        }

        builder.Services.AddScoped(sp =>
        {
            var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
            handler.InnerHandler = new HttpClientHandler();
            return new HttpClient(handler)
            {
                BaseAddress = new Uri(apiBaseUrl)
            };
        });

        // Register Application Services
        builder.Services.AddScoped<AuthService>();
        builder.Services.AddScoped<CustomerApiService>();

        await builder.Build().RunAsync();
    }
}
