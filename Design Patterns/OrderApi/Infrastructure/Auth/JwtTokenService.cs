using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OrderApi.Builders;
using OrderApi.Domain.Entities;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Builder][SOLID: SRP][SOLID: DIP] Generates and validates JWTs without leaking token-library details to callers.</summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public int AccessTokenExpiryMinutes => GetRequiredInt("JwtSettings:AccessTokenExpiryMinutes", 15);

    public string GenerateAccessToken(User user, string? provider = null)
    {
        var now = DateTime.UtcNow;
        var claimsBuilder = new JwtClaimsBuilder()
            .WithSubject(user.Id.ToString())
            .WithEmail(user.Email)
            .WithRole(user.Role.ToString())
            .WithJti(Guid.NewGuid().ToString("N"))
            .WithIssuedAt(now);

        if (!string.IsNullOrWhiteSpace(provider))
        {
            claimsBuilder.WithCustomClaim("provider", provider);
        }

        var claims = claimsBuilder.Build();
        var credentials = new SigningCredentials(GetSigningKey(), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"],
            audience: _configuration["JwtSettings:Audience"],
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(AccessTokenExpiryMinutes),
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomPart = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var timestampPart = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        return $"{randomPart}.{timestampPart}";
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JwtSettings:Issuer"],
                ValidAudience = _configuration["JwtSettings:Audience"],
                IssuerSigningKey = GetSigningKey(),
                ClockSkew = TimeSpan.Zero
            };

            return _tokenHandler.ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }

    private SymmetricSecurityKey GetSigningKey()
    {
        var secret = _configuration["JwtSettings:SecretKey"] ?? string.Empty;
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }

    private int GetRequiredInt(string key, int fallback)
    {
        return int.TryParse(_configuration[key], out var value) ? value : fallback;
    }
}