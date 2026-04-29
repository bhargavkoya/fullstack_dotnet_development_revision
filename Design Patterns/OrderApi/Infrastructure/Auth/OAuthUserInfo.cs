namespace OrderApi.Infrastructure.Auth;

/// <summary>[Pattern: Template Method][SOLID: SRP] Represents the normalized user information returned by an OAuth provider.</summary>
public sealed record OAuthUserInfo(string Provider, string ProviderUserId, string Email, string? DisplayName);