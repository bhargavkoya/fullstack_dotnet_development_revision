using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OrderApi.Builders;

/// <summary>[Pattern: Builder][SOLID: SRP] Builds a JWT claim set through a fluent API.</summary>
public sealed class JwtClaimsBuilder
{
    private readonly List<Claim> _claims = [];

    public JwtClaimsBuilder WithSubject(string subject)
    {
        _claims.Add(new Claim(ClaimTypes.NameIdentifier, subject));
        return this;
    }

    public JwtClaimsBuilder WithEmail(string email)
    {
        _claims.Add(new Claim(ClaimTypes.Email, email));
        return this;
    }

    public JwtClaimsBuilder WithRole(string role)
    {
        _claims.Add(new Claim(ClaimTypes.Role, role));
        return this;
    }

    public JwtClaimsBuilder WithJti(string jti)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jti));
        return this;
    }

    public JwtClaimsBuilder WithIssuedAt(DateTime issuedAtUtc)
    {
        _claims.Add(new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(issuedAtUtc).ToUnixTimeSeconds().ToString()));
        return this;
    }

    public JwtClaimsBuilder WithCustomClaim(string key, string value)
    {
        _claims.Add(new Claim(key, value));
        return this;
    }

    public IReadOnlyCollection<Claim> Build() => _claims.AsReadOnly();
}