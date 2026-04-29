using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Template Method][SOLID: OCP] Defines the OAuth flow skeleton while providers override provider-specific steps.</summary>
public abstract class OAuthProviderBase : IOAuthProvider
{
    protected readonly HttpClient _httpClient;
    protected readonly IConfiguration _configuration;

    protected OAuthProviderBase(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public abstract string ProviderName { get; }

    protected abstract string ClientId { get; }

    protected abstract string ClientSecret { get; }

    protected abstract string RedirectUri { get; }

    protected abstract string AuthorizationEndpoint { get; }

    protected abstract string TokenEndpoint { get; }

    protected abstract string UserInfoEndpoint { get; }

    protected abstract string Scope { get; }

    public string GetAuthorizationUrl(string state)
    {
        if (UsesPlaceholderCredentials())
        {
            return $"https://example.com/oauth/{ProviderName}?state={Uri.EscapeDataString(state)}";
        }

        var query = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["redirect_uri"] = RedirectUri,
            ["response_type"] = "code",
            ["scope"] = Scope,
            ["state"] = state
        };

        return QueryStringHelper(AuthorizationEndpoint, query);
    }

    public async Task<OAuthUserInfo> AuthenticateAsync(string code, CancellationToken cancellationToken = default)
    {
        if (UsesPlaceholderCredentials())
        {
            return BuildPlaceholderUserInfo(code);
        }

        var accessToken = await ExchangeCodeForToken(code, cancellationToken).ConfigureAwait(false);
        var rawProfile = await FetchRawUserProfile(accessToken, cancellationToken).ConfigureAwait(false);
        return MapToOAuthUserInfo(rawProfile);
    }

    protected abstract Task<string> ExchangeCodeForToken(string code, CancellationToken cancellationToken);

    protected abstract Task<JsonElement> FetchRawUserProfile(string accessToken, CancellationToken cancellationToken);

    protected abstract OAuthUserInfo MapToOAuthUserInfo(JsonElement rawProfile);

    protected virtual bool UsesPlaceholderCredentials()
    {
        return string.IsNullOrWhiteSpace(ClientId)
            || ClientId.Contains("your-", StringComparison.OrdinalIgnoreCase)
            || ClientSecret.Contains("your-", StringComparison.OrdinalIgnoreCase);
    }

    private static string QueryStringHelper(string baseUrl, IReadOnlyDictionary<string, string> parameters)
    {
        var builder = new UriBuilder(baseUrl);
        var query = string.Join("&", parameters.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value)}"));
        builder.Query = query;
        return builder.ToString();
    }

    protected virtual OAuthUserInfo BuildPlaceholderUserInfo(string code)
    {
        return new OAuthUserInfo(ProviderName, $"placeholder-{code}", $"{ProviderName}+{code}@example.com", $"{ProviderName.ToUpperInvariant()} User");
    }

    protected async Task<string> PostFormForTokenAsync(Dictionary<string, string> formValues, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, TokenEndpoint)
        {
            Content = new FormUrlEncodedContent(formValues)
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    protected async Task<JsonElement> GetJsonAsync(string url, string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd("OrderApi/1.0");

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return JsonDocument.Parse(payload).RootElement.Clone();
    }
}