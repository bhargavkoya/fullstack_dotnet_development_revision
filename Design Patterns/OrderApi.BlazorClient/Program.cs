using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using MudBlazor.Services;
using OrderApi.BlazorClient.Auth;
using OrderApi.BlazorClient.Builders;
using OrderApi.BlazorClient.Factories;
using OrderApi.BlazorClient.HttpClients;
using OrderApi.BlazorClient.Services;
using OrderApi.BlazorClient.State;
using OrderApi.BlazorClient;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ── LocalStorage ──────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ── Auth State [Pattern: Proxy] [SRP] ────────────────────────────────
builder.Services.AddScoped<TokenManager>();
builder.Services.AddScoped<JwtAuthStateProvider>(sp =>
{
	var provider = new JwtAuthStateProvider(sp.GetRequiredService<TokenManager>());
	provider.RefreshAccessTokenAsync = async () =>
	{
		var authService = sp.GetRequiredService<IAuthService>();
		var refreshed = await authService.RefreshAsync();
		return refreshed?.AccessToken;
	};

	return provider;
});
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
	sp.GetRequiredService<JwtAuthStateProvider>()); // [DIP] Bind abstraction
builder.Services.AddScoped<IAuthStateProvider>(sp => sp.GetRequiredService<JwtAuthStateProvider>());
builder.Services.AddAuthorizationCore();

// ── HTTP Handlers [Pattern: Decorator] [SRP] ─────────────────────────
builder.Services.AddTransient<AuthenticatedHttpHandler>(sp =>
{
	var handler = new AuthenticatedHttpHandler(
		sp.GetRequiredService<TokenManager>(),
		sp.GetRequiredService<JwtAuthStateProvider>(),
		sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>());

	handler.TryRefreshTokensAsync = async () =>
	{
		var authService = sp.GetRequiredService<IAuthService>();
		return await authService.RefreshAsync() is not null;
	};

	return handler;
});

// ── Typed HTTP Clients [Pattern: Template Method] [ISP] ──────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
	client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
{
	client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthenticatedHttpHandler>();

// ── State Management [Pattern: Observer] ─────────────────────────────
builder.Services.AddScoped<AppState>();
builder.Services.AddScoped<IAppState>(sp => sp.GetRequiredService<AppState>());

// ── Application Services [SRP] [DIP] ─────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// ── Factories + Builders [Pattern: Factory] [Pattern: Builder] ────────
builder.Services.AddScoped<OrderRequestBuilderFactory>();

// ── MudBlazor ─────────────────────────────────────────────────────────
builder.Services.AddMudServices(config =>
{
	config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
	config.SnackbarConfiguration.MaxDisplayedSnackbars = 3;
});

await builder.Build().RunAsync();
