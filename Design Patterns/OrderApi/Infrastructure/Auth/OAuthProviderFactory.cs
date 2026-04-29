using Microsoft.Extensions.DependencyInjection;

namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Factory][Pattern: OCP] Resolves the correct OAuth provider implementation from a provider name.</summary>
public sealed class OAuthProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public OAuthProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IOAuthProvider Create(string provider) => provider.ToLowerInvariant() switch
    {
        "google" => _serviceProvider.GetRequiredService<GoogleOAuthProvider>(),
        "github" => _serviceProvider.GetRequiredService<GitHubOAuthProvider>(),
        _ => throw new NotSupportedException($"OAuth provider '{provider}' is not supported")
    };
}