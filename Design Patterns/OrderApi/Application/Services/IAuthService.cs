using OrderApi.Application.Dtos;
using OrderApi.Domain.Entities;

namespace OrderApi.Application.Services;

/// <summary>[SOLID: DIP][SOLID: SRP] Orchestrates authentication flows (local, OAuth, refresh, logout).</summary>
public interface IAuthService
{
    Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
}
