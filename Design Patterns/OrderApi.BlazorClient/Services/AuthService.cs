using OrderApi.BlazorClient.Auth;
using OrderApi.BlazorClient.HttpClients;
using OrderApi.BlazorClient.Models.Requests;
using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.Services;

/// <summary>[Pattern: SRP][DIP] Orchestrates authentication flows across API, token storage, and auth state.</summary>
public sealed class AuthService : IAuthService
{
    private readonly IAuthApiClient _authApiClient;
    private readonly TokenManager _tokenManager;
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthService(
        IAuthApiClient authApiClient,
        TokenManager tokenManager,
        JwtAuthStateProvider authStateProvider)
    {
        _authApiClient = authApiClient;
        _tokenManager = tokenManager;
        _authStateProvider = authStateProvider;
    }

    /// <inheritdoc />
    public async Task<TokenResponse?> LoginAsync(string email, string password)
    {
        var response = await _authApiClient.LoginAsync(new LoginRequest(email, password));
        if (response is null)
        {
            return null;
        }

        await PersistTokensAsync(response);
        await _authStateProvider.MarkUserAsAuthenticated(response.AccessToken);
        return response;
    }

    /// <inheritdoc />
    public async Task<TokenResponse?> RefreshAsync()
    {
        var refreshToken = await _tokenManager.GetRefreshTokenAsync();
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var response = await _authApiClient.RefreshAsync(new RefreshTokenRequest(refreshToken));
        if (response is null)
        {
            return null;
        }

        await PersistTokensAsync(response);
        await _authStateProvider.MarkUserAsAuthenticated(response.AccessToken);
        return response;
    }

    /// <inheritdoc />
    public async Task LogoutAsync()
    {
        var refreshToken = await _tokenManager.GetRefreshTokenAsync();
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _authApiClient.LogoutAsync(refreshToken);
        }

        await _authStateProvider.MarkUserAsLoggedOut();
    }

    /// <inheritdoc />
    public Task<string?> GetOAuthUrlAsync(string provider) => _authApiClient.GetOAuthUrlAsync(provider);

    private async Task PersistTokensAsync(TokenResponse response)
    {
        await _tokenManager.SaveTokensAsync(response.AccessToken, response.RefreshToken);
    }
}