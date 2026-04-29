using OrderApi.Domain.Enums;

namespace OrderApi.Domain.Entities;

/// <summary>[SOLID: SRP] Represents an application user for both local and OAuth authentication flows.</summary>
public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Customer;

    public string? OAuthProvider { get; set; }

    public string? OAuthSubject { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}