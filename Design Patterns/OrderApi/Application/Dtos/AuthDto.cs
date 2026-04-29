namespace OrderApi.Application.Dtos;

/// <summary>Data transfer object for login requests.</summary>
public sealed record LoginRequest(string Email, string Password);

/// <summary>Data transfer object for authentication token responses.</summary>
public sealed record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);

/// <summary>Data transfer object for refresh token requests.</summary>
public sealed record RefreshTokenRequest(string RefreshToken);

/// <summary>Data transfer object for logout requests.</summary>
public sealed record LogoutRequest(string RefreshToken);
