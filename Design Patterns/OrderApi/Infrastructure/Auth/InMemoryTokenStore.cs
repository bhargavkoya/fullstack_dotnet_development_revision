using System.Collections.Concurrent;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Repository][SOLID: ISP][SOLID: SRP] Keeps refresh tokens and blacklisted access-token JTIs in memory for local development.</summary>
public sealed class InMemoryTokenStore : IRefreshTokenStore, IAccessTokenBlacklist
{
    private readonly ConcurrentDictionary<string, RefreshTokenRecord> _refreshTokens = new();
    private readonly ConcurrentDictionary<string, byte> _blacklistedJtis = new();

    public void StoreRefreshToken(string userId, string refreshToken, DateTime expiresAtUtc)
    {
        _refreshTokens[refreshToken] = new RefreshTokenRecord(userId, expiresAtUtc);
    }

    public bool ValidateRefreshToken(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var record))
        {
            return false;
        }

        if (record.ExpiresAtUtc <= DateTime.UtcNow)
        {
            _refreshTokens.TryRemove(refreshToken, out _);
            return false;
        }

        return true;
    }

    public void RevokeRefreshToken(string refreshToken)
    {
        _refreshTokens.TryRemove(refreshToken, out _);
    }

    public string? GetUserIdForRefreshToken(string refreshToken)
    {
        return ValidateRefreshToken(refreshToken) && _refreshTokens.TryGetValue(refreshToken, out var record)
            ? record.UserId
            : null;
    }

    public void Blacklist(string jti)
    {
        _blacklistedJtis[jti] = 0;
    }

    public bool IsBlacklisted(string jti)
    {
        return _blacklistedJtis.ContainsKey(jti);
    }

    private sealed record RefreshTokenRecord(string UserId, DateTime ExpiresAtUtc);
}