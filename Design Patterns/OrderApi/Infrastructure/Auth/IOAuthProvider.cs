namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Template Method][Pattern: Factory][SOLID: OCP] Defines a provider that can build OAuth URLs and complete an OAuth exchange.</summary>
public interface IOAuthProvider
{
    string ProviderName { get; }

    string GetAuthorizationUrl(string state);

    Task<OAuthUserInfo> AuthenticateAsync(string code, CancellationToken cancellationToken = default);
}