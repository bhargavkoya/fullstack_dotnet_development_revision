namespace OrderApi.Infrastructure.Auth;

/// <summary>[SOLID: ISP][SOLID: SRP] Maintains blacklisted access-token identifiers independently from refresh-token storage.</summary>
public interface IAccessTokenBlacklist
{
    void Blacklist(string jti);

    bool IsBlacklisted(string jti);
}