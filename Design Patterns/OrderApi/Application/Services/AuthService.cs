using System.Security.Cryptography;
using System.Text;
using OrderApi.Application.Dtos;
using OrderApi.Domain.Entities;
using OrderApi.Infrastructure.Auth;
using OrderApi.Infrastructure.Repositories;

namespace OrderApi.Application.Services;

/// <summary>[SOLID: SRP][SOLID: DIP] Implements authentication orchestration without exposing token or persistence details.</summary>
public sealed class AuthService : IAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly IAccessTokenBlacklist _accessTokenBlacklist;

    public AuthService(
        IJwtTokenService jwtTokenService,
        IUserRepository userRepository,
        IRefreshTokenStore refreshTokenStore,
        IAccessTokenBlacklist accessTokenBlacklist)
    {
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _refreshTokenStore = refreshTokenStore;
        _accessTokenBlacklist = accessTokenBlacklist;
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException("Email and password are required.");
        }

        // Find user by email
        var user = await _userRepository.FindByEmailAsync(request.Email, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Validate password
        var providedHash = HashPassword(request.Password);
        if (user.PasswordHash != providedHash)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Store refresh token
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _refreshTokenStore.StoreRefreshToken(user.Id.ToString(), refreshToken, refreshTokenExpiry);

        return new TokenResponse(accessToken, refreshToken, _jwtTokenService.AccessTokenExpiryMinutes * 60);
    }

    public async Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        // Validate the refresh token
        if (!_refreshTokenStore.ValidateRefreshToken(request.RefreshToken))
        {
            throw new UnauthorizedAccessException("Refresh token is invalid or expired.");
        }

        // Get user ID from stored refresh token
        var userId = _refreshTokenStore.GetUserIdForRefreshToken(request.RefreshToken);

        if (!Guid.TryParse(userId, out var userGuid))
        {
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        // Fetch the user
        var user = await _userRepository.FindByIdAsync(userGuid, cancellationToken).ConfigureAwait(false);

        if (user is null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        // Revoke old refresh token and generate new ones
        _refreshTokenStore.RevokeRefreshToken(request.RefreshToken);

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        _refreshTokenStore.StoreRefreshToken(user.Id.ToString(), newRefreshToken, newRefreshTokenExpiry);

        return new TokenResponse(newAccessToken, newRefreshToken, _jwtTokenService.AccessTokenExpiryMinutes * 60);
    }

    public Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new ArgumentException("Refresh token is required.");
        }

        // Revoke the refresh token
        _refreshTokenStore.RevokeRefreshToken(request.RefreshToken);

        return Task.CompletedTask;
    }

    private static string HashPassword(string password)
    {
        // Simple SHA256 hash for demo purposes.
        // In production, use BCrypt or ASP.NET Core Identity password hasher.
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(hashedBytes);
    }
}
