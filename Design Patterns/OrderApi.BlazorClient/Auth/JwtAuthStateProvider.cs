using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace OrderApi.BlazorClient.Auth;

/// <summary>[Pattern: Proxy][SRP] Controls access to auth state and token parsing for Blazor authentication.</summary>
public sealed class JwtAuthStateProvider : AuthenticationStateProvider, IAuthStateProvider
{
    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly TokenManager _tokenManager;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    /// <summary>
    /// [OCP] Hook for silent refresh; wired by auth orchestration in a later step.
    /// Should return a new access token when refresh succeeds, otherwise null.
    /// </summary>
    public Func<Task<string?>>? RefreshAccessTokenAsync { get; set; }

    public JwtAuthStateProvider(TokenManager tokenManager)
    {
        _tokenManager = tokenManager;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var accessToken = await _tokenManager.GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return AnonymousState;
        }

        var claims = ParseClaimsFromJwt(accessToken);

        // Proactive refresh near expiry to avoid mid-request token expiry.
        if (IsTokenExpiringSoon(claims))
        {
            var refreshedToken = await TryRefreshAccessTokenAsync();
            if (!string.IsNullOrWhiteSpace(refreshedToken))
            {
                accessToken = refreshedToken;
                claims = ParseClaimsFromJwt(accessToken);
            }
            else
            {
                await MarkUserAsLoggedOut();
                return AnonymousState;
            }
        }

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public Task MarkUserAsAuthenticated(string accessToken)
    {
        var identity = new ClaimsIdentity(ParseClaimsFromJwt(accessToken), authenticationType: "jwt");
        var user = new ClaimsPrincipal(identity);
        var state = new AuthenticationState(user);

        NotifyAuthenticationStateChanged(Task.FromResult(state));
        return Task.CompletedTask;
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _tokenManager.ClearTokensAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
    }

    private async Task<string?> TryRefreshAccessTokenAsync()
    {
        if (RefreshAccessTokenAsync is null)
        {
            return null;
        }

        await _refreshLock.WaitAsync();
        try
        {
            return await RefreshAccessTokenAsync.Invoke();
        }
        catch
        {
            return null;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static bool IsTokenExpiringSoon(IEnumerable<Claim> claims)
    {
        var expValue = claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        if (!long.TryParse(expValue, out var expUnix))
        {
            return true;
        }

        var expiryUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
        return expiryUtc <= DateTime.UtcNow.AddMinutes(2);
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();

        var parts = jwt.Split('.');
        if (parts.Length < 2)
        {
            return claims;
        }

        var payloadJson = DecodeBase64(parts[1]);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);
        if (keyValuePairs is null)
        {
            return claims;
        }

        foreach (var pair in keyValuePairs)
        {
            if (pair.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in pair.Value.EnumerateArray())
                {
                    claims.Add(new Claim(MapClaimType(pair.Key), item.ToString()));
                }

                continue;
            }

            claims.Add(new Claim(MapClaimType(pair.Key), pair.Value.ToString()));
        }

        return claims;
    }

    private static string MapClaimType(string claimType) => claimType switch
    {
        "sub" => ClaimTypes.NameIdentifier,
        "email" => ClaimTypes.Email,
        "role" => ClaimTypes.Role,
        "jti" => "jti",
        "exp" => "exp",
        _ => claimType
    };

    private static string DecodeBase64(string payload)
    {
        payload = payload.Replace('-', '+').Replace('_', '/');

        switch (payload.Length % 4)
        {
            case 2:
                payload += "==";
                break;
            case 3:
                payload += "=";
                break;
        }

        var bytes = Convert.FromBase64String(payload);
        return System.Text.Encoding.UTF8.GetString(bytes);
    }
}
