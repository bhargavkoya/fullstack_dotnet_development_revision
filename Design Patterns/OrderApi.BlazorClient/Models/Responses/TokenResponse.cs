namespace OrderApi.BlazorClient.Models.Responses;

/// <summary>[SRP] Represents authentication tokens returned by the API.</summary>
public sealed record TokenResponse(string AccessToken, string RefreshToken, int ExpiresIn);
