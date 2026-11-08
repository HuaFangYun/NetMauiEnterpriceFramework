using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using __PROJECT__.Shared.Handlers;
using __PROJECT__.Shared.Services;
using __PROJECT__.Shared.Services.Interfaces;
using __PROJECT__.Shared.Utils;
using __PROJECT__.Web;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7143";

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<ITokenStorage, TokenStorage>();
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddScoped<INotifyAuthenticationState>(sp => sp.GetRequiredService<JwtAuthStateProvider>());

builder.Services.AddSingleton<DynamicComponentResolver>();
builder.Services.AddScoped<TabStateService>();
builder.Services.AddScoped<AuthenticatingHttpHandler>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<SettingApiService>();
builder.Services.AddScoped<ApprovalApiService>();
builder.Services.AddScoped<ParameterApiService>();
builder.Services.AddScoped<DynamicApiService>();
builder.Services.AddScoped<NotificationApiService>();
builder.Services.AddScoped<ExportApiService>();
builder.Services.AddScoped<ScreenMenuApiService>();
builder.Services.AddScoped<IExcelFilePicker, NoopExcelFilePicker>();

builder.Services.AddHttpClient("ApiClient", client => client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthenticatingHttpHandler>();

await builder.Build().RunAsync();
