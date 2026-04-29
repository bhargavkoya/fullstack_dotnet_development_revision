using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.Dtos;
using OrderApi.Application.Services;
using OrderApi.Controllers;
using OrderApi.Infrastructure.Auth;

namespace OrderApi.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task LoginAsync_ReturnsOk_WithTokenResponse()
    {
        var authService = new FakeAuthService();
        var controller = new AuthController(authService, new OAuthProviderFactory(new ServiceProviderStub()));

        var result = await controller.LoginAsync(new LoginRequest("admin@orderapi.com", "Admin123!"), CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tokenResponse = Assert.IsType<TokenResponse>(okResult.Value);
        Assert.Equal("access-token", tokenResponse.AccessToken);
        Assert.Equal("refresh-token", tokenResponse.RefreshToken);
    }

    private sealed class FakeAuthService : IAuthService
    {
        public Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new TokenResponse("access-token", "refresh-token", 900));

        public Task<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new TokenResponse("new-access-token", "new-refresh-token", 900));

        public Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class ServiceProviderStub : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
