using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.Services;

/// <summary>[Pattern: ISP][SRP][DIP] Exposes only auth orchestration operations for the UI layer.</summary>
public interface IAuthService
{
    /// <summary>[ISP] Logs a user in and persists the returned tokens.</summary>
    Task<TokenResponse?> LoginAsync(string email, string password);

    /// <summary>[ISP] Refreshes the current access token using the stored refresh token.</summary>
    Task<TokenResponse?> RefreshAsync();

    /// <summary>[ISP] Logs the current user out and clears persisted tokens.</summary>
    Task LogoutAsync();

    /// <summary>[ISP] Retrieves an OAuth authorization URL for the supplied provider.</summary>
    Task<string?> GetOAuthUrlAsync(string provider);
}