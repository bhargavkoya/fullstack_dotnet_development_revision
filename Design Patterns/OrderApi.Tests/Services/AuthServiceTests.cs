using OrderApi.Application.Dtos;
using OrderApi.Application.Services;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Infrastructure.Auth;
using OrderApi.Infrastructure.Repositories;

namespace OrderApi.Tests.Services;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ReturnsTokens_ForValidCredentials()
    {
        var user = new User
        {
            Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            Email = "admin@orderapi.com",
            PasswordHash = HashPassword("Admin123!"),
            Role = UserRole.Admin
        };

        var service = new AuthService(
            new FakeJwtTokenService(),
            new FakeUserRepository(user),
            new InMemoryTokenStore(),
            new InMemoryTokenStore());

        var response = await service.LoginAsync(new LoginRequest("admin@orderapi.com", "Admin123!"));

        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public int AccessTokenExpiryMinutes => 15;

        public string GenerateAccessToken(User user, string? provider = null) => "access-token";

        public string GenerateRefreshToken() => "refresh-token";

        public System.Security.Claims.ClaimsPrincipal? ValidateToken(string token) => null;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly User _user;

        public FakeUserRepository(User user) => _user = user;

        public Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(string.Equals(email, _user.Email, StringComparison.OrdinalIgnoreCase) ? _user : null);

        public Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<User?>(_user.Id == id ? _user : null);

        public Task AddAsync(User user, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
