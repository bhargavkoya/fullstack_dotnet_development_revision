using System.Net.Http.Json;
using OrderApi.BlazorClient.Models.Requests;
using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.HttpClients;

/// <summary>[Pattern: Template Method][LSP] Auth HTTP client built on ApiClientBase with standardized pipeline behavior.</summary>
public sealed class AuthApiClient : ApiClientBase, IAuthApiClient
{
    public AuthApiClient(HttpClient http)
        : base(http)
    {
    }

    public Task<TokenResponse?> LoginAsync(LoginRequest request) =>
        ExecuteAsync<TokenResponse>(() => Http.PostAsJsonAsync("/api/auth/login", request));

    public Task<TokenResponse?> RefreshAsync(RefreshTokenRequest request) =>
        ExecuteAsync<TokenResponse>(() => Http.PostAsJsonAsync("/api/auth/refresh", request));

    public async Task LogoutAsync(string refreshToken)
    {
        await ExecuteAsync<object?>(() =>
            Http.PostAsJsonAsync("/api/auth/logout", new RefreshTokenRequest(refreshToken)));
    }

    public async Task<string?> GetOAuthUrlAsync(string provider)
    {
        var result = await ExecuteAsync<OAuthAuthorizationResponse>(() =>
            Http.GetAsync($"/api/auth/oauth/{provider}"));

        return result?.AuthorizationUrl;
    }

    public Task<TokenResponse?> OAuthCallbackAsync(string provider, string code, string state) =>
        ExecuteAsync<TokenResponse>(() =>
            Http.GetAsync($"/api/auth/oauth/{provider}/callback?code={Uri.EscapeDataString(code)}&state={Uri.EscapeDataString(state)}"));

    private sealed record OAuthAuthorizationResponse(string AuthorizationUrl);
}
