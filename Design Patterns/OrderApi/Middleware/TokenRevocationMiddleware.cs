using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Http;
using OrderApi.Infrastructure.Auth;

namespace OrderApi.Middleware;

/// <summary>[Pattern: Chain of Responsibility][SOLID: SRP] Intercepts requests to check if the access token JTI has been blacklisted.</summary>
public sealed class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenRevocationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAccessTokenBlacklist blacklist)
    {
        // Extract the Bearer token from the Authorization header without full validation
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            // Decode the JWT without validation to extract the JTI claim
            var jti = ExtractJtiFromToken(token);

            if (!string.IsNullOrEmpty(jti) && blacklist.IsBlacklisted(jti))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked." }).ConfigureAwait(false);
                return;
            }
        }

        // Call the next middleware in the chain
        await _next(context).ConfigureAwait(false);
    }

    private static string? ExtractJtiFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();

            // Read without validation—we only need the JTI claim for revocation check
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
