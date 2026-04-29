namespace OrderApi.Infrastructure.Auth;

/// <summary>[SOLID: ISP][SOLID: SRP] Stores and validates refresh tokens without exposing blacklist concerns.</summary>
public interface IRefreshTokenStore
{
    void StoreRefreshToken(string userId, string refreshToken, DateTime expiresAtUtc);

    bool ValidateRefreshToken(string refreshToken);

    void RevokeRefreshToken(string refreshToken);

    string? GetUserIdForRefreshToken(string refreshToken);
}