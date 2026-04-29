using Blazored.LocalStorage;

namespace OrderApi.BlazorClient.Auth;

/// <summary>[SRP] Responsible for token persistence and retrieval only.</summary>
public sealed class TokenManager
{
    private const string AccessTokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";
    private readonly ILocalStorageService _localStorage;

    public TokenManager(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken)
    {
        await _localStorage.SetItemAsync(AccessTokenKey, accessToken);
        await _localStorage.SetItemAsync(RefreshTokenKey, refreshToken);
    }

    public Task<string?> GetAccessTokenAsync() => _localStorage.GetItemAsync<string>(AccessTokenKey);

    public Task<string?> GetRefreshTokenAsync() => _localStorage.GetItemAsync<string>(RefreshTokenKey);

    public async Task ClearTokensAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
    }
}
