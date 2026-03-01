using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using VMWorkflow.Web;
using VMWorkflow.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<JwtAuthorizationHandler>();

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<JwtAuthorizationHandler>();
    handler.InnerHandler = new HttpClientHandler();
    var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5028";
    return new HttpClient(handler) { BaseAddress = new Uri(apiBase) };
});

builder.Services.AddScoped<IApiClient, ApiClient>();

await builder.Build().RunAsync();
