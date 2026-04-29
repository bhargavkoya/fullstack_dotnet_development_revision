using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Proxy][SOLID: SRP][SOLID: OCP] Caches validated JWT principals to avoid repeating expensive signature verification.</summary>
public sealed class JwtTokenValidatorProxy : IJwtTokenService
{
    private readonly IJwtTokenService _inner;
    private readonly IMemoryCache _cache;

    public JwtTokenValidatorProxy(IJwtTokenService inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public int AccessTokenExpiryMinutes => _inner.AccessTokenExpiryMinutes;

    public string GenerateAccessToken(OrderApi.Domain.Entities.User user, string? provider = null)
        => _inner.GenerateAccessToken(user, provider);

    public string GenerateRefreshToken() => _inner.GenerateRefreshToken();

    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (_cache.TryGetValue(token, out ClaimsPrincipal? cachedPrincipal))
        {
            return cachedPrincipal;
        }

        var principal = _inner.ValidateToken(token);
        if (principal is null)
        {
            return null;
        }

        _cache.Set(token, principal, TimeSpan.FromMinutes(1));
        return principal;
    }
}