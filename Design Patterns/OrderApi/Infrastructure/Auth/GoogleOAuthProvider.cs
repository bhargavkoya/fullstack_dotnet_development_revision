using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Template Method][Pattern: Factory][SOLID: OCP] Completes the Google OAuth flow using Google-specific endpoints and mapping rules.</summary>
public sealed class GoogleOAuthProvider : OAuthProviderBase
{
    public GoogleOAuthProvider(HttpClient httpClient, IConfiguration configuration)
        : base(httpClient, configuration)
    {
    }

    public override string ProviderName => "google";

    protected override string ClientId => _configuration["OAuthSettings:Google:ClientId"] ?? string.Empty;

    protected override string ClientSecret => _configuration["OAuthSettings:Google:ClientSecret"] ?? string.Empty;

    protected override string RedirectUri => _configuration["OAuthSettings:Google:RedirectUri"] ?? string.Empty;

    protected override string AuthorizationEndpoint => "https://accounts.google.com/o/oauth2/v2/auth";

    protected override string TokenEndpoint => "https://oauth2.googleapis.com/token";

    protected override string UserInfoEndpoint => "https://openidconnect.googleapis.com/v1/userinfo";

    protected override string Scope => "openid email profile";

    protected override async Task<string> ExchangeCodeForToken(string code, CancellationToken cancellationToken)
    {
        var formValues = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["code"] = code,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = RedirectUri
        };

        var payload = await PostFormForTokenAsync(formValues, cancellationToken).ConfigureAwait(false);
        using var json = JsonDocument.Parse(payload);
        return json.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
    }

    protected override async Task<JsonElement> FetchRawUserProfile(string accessToken, CancellationToken cancellationToken)
    {
        return await GetJsonAsync(UserInfoEndpoint, accessToken, cancellationToken).ConfigureAwait(false);
    }

    protected override OAuthUserInfo MapToOAuthUserInfo(JsonElement rawProfile)
    {
        var subject = rawProfile.TryGetProperty("sub", out var subValue) ? subValue.GetString() : null;
        var email = rawProfile.TryGetProperty("email", out var emailValue) ? emailValue.GetString() : null;
        var name = rawProfile.TryGetProperty("name", out var nameValue) ? nameValue.GetString() : null;

        return new OAuthUserInfo(ProviderName, subject ?? string.Empty, email ?? string.Empty, name);
    }
}