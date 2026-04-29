using System.Security.Claims;
using OrderApi.Domain.Entities;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Proxy][SOLID: DIP] Defines JWT generation and validation behavior behind an abstraction.</summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(User user, string? provider = null);

    string GenerateRefreshToken();

    ClaimsPrincipal? ValidateToken(string token);

    int AccessTokenExpiryMinutes { get; }
}