using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Template Method][Pattern: Factory][SOLID: OCP] Completes the GitHub OAuth flow using GitHub-specific endpoints and mapping rules.</summary>
public sealed class GitHubOAuthProvider : OAuthProviderBase
{
    public GitHubOAuthProvider(HttpClient httpClient, IConfiguration configuration)
        : base(httpClient, configuration)
    {
    }

    public override string ProviderName => "github";

    protected override string ClientId => _configuration["OAuthSettings:GitHub:ClientId"] ?? string.Empty;

    protected override string ClientSecret => _configuration["OAuthSettings:GitHub:ClientSecret"] ?? string.Empty;

    protected override string RedirectUri => _configuration["OAuthSettings:GitHub:RedirectUri"] ?? string.Empty;

    protected override string AuthorizationEndpoint => "https://github.com/login/oauth/authorize";

    protected override string TokenEndpoint => "https://github.com/login/oauth/access_token";

    protected override string UserInfoEndpoint => "https://api.github.com/user";

    protected override string Scope => "read:user user:email";

    protected override async Task<string> ExchangeCodeForToken(string code, CancellationToken cancellationToken)
    {
        var formValues = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = RedirectUri
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(formValues)
        };

        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("OrderApi/1.0");

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        using var json = JsonDocument.Parse(payload);
        return json.RootElement.GetProperty("access_token").GetString() ?? string.Empty;
    }

    protected override async Task<JsonElement> FetchRawUserProfile(string accessToken, CancellationToken cancellationToken)
    {
        return await GetJsonAsync(UserInfoEndpoint, accessToken, cancellationToken).ConfigureAwait(false);
    }

    protected override OAuthUserInfo MapToOAuthUserInfo(JsonElement rawProfile)
    {
        var subject = rawProfile.TryGetProperty("id", out var idValue) ? idValue.GetRawText() : null;
        var email = rawProfile.TryGetProperty("email", out var emailValue) ? emailValue.GetString() : null;
        var name = rawProfile.TryGetProperty("name", out var nameValue) ? nameValue.GetString() : null;

        return new OAuthUserInfo(ProviderName, subject ?? string.Empty, email ?? string.Empty, name);
    }

    protected override OAuthUserInfo BuildPlaceholderUserInfo(string code)
    {
        return new OAuthUserInfo(ProviderName, $"github-{code}", $"github+{code}@example.com", "GITHUB User");
    }
}