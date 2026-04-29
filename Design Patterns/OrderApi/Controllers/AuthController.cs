using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.Dtos;
using OrderApi.Application.Services;
using OrderApi.Infrastructure.Auth;

namespace OrderApi.Controllers;

/// <summary>[SOLID: SRP] Handles authentication endpoints: login, token refresh, logout, and OAuth redirects.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly OAuthProviderFactory _oAuthProviderFactory;

    public AuthController(IAuthService authService, OAuthProviderFactory oAuthProviderFactory)
    {
        _authService = authService;
        _oAuthProviderFactory = oAuthProviderFactory;
    }

    /// <summary>Local login with email and password.</summary>
    /// <param name="request">Email and password credentials.</param>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken).ConfigureAwait(false);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Refresh an expired access token using a valid refresh token.</summary>
    /// <param name="request">Refresh token.</param>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> RefreshTokenAsync([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request, cancellationToken).ConfigureAwait(false);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Logout by blacklisting the current refresh token.</summary>
    /// <param name="request">Refresh token to revoke.</param>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> LogoutAsync([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await _authService.LogoutAsync(request, cancellationToken).ConfigureAwait(false);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get the OAuth authorization URL for the specified provider.</summary>
    /// <param name="provider">OAuth provider name (google, github).</param>
    [HttpGet("oauth/{provider}")]
    [AllowAnonymous]
    public IActionResult GetAuthorizationUrl(string provider)
    {
        try
        {
            var oAuthProvider = _oAuthProviderFactory.Create(provider);
            var state = Guid.NewGuid().ToString("N");

            // Store state in session or cache for callback validation (simplified here)
            var authorizationUrl = oAuthProvider.GetAuthorizationUrl(state);

            return Ok(new { authorizationUrl });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>OAuth callback endpoint that exchanges authorization code for tokens.</summary>
    /// <param name="provider">OAuth provider name.</param>
    /// <param name="code">Authorization code from OAuth provider.</param>
    /// <param name="state">State parameter for CSRF validation.</param>
    [HttpGet("oauth/{provider}/callback")]
    [AllowAnonymous]
    public async Task<IActionResult> OAuthCallbackAsync(string provider, [FromQuery] string code, [FromQuery] string state, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "Authorization code is missing." });
            }

            var oAuthProvider = _oAuthProviderFactory.Create(provider);

            // Exchange code for user info
            var userInfo = await oAuthProvider.AuthenticateAsync(code, cancellationToken).ConfigureAwait(false);

            // In a production flow, here you would:
            // 1. Upsert the user to the database
            // 2. Generate JWT tokens
            // 3. Return tokens to the client

            // For this demo, return the user info
            return Ok(new
            {
                message = "OAuth flow would complete here in production.",
                userInfo = new
                {
                    provider = userInfo.Provider,
                    providerUserId = userInfo.ProviderUserId,
                    email = userInfo.Email,
                    displayName = userInfo.DisplayName
                }
            });
        }
        catch (NotSupportedException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
