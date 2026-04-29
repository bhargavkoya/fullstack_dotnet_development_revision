using OrderApi.BlazorClient.Models.Requests;
using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.HttpClients;

/// <summary>[ISP][OCP] Exposes only auth-related HTTP operations for authentication workflows.</summary>
public interface IAuthApiClient
{
    Task<TokenResponse?> LoginAsync(LoginRequest request);
    Task<TokenResponse?> RefreshAsync(RefreshTokenRequest request);
    Task LogoutAsync(string refreshToken);
    Task<string?> GetOAuthUrlAsync(string provider);
    Task<TokenResponse?> OAuthCallbackAsync(string provider, string code, string state);
}
