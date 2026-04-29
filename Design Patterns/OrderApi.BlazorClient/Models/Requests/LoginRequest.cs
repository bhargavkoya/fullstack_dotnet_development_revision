namespace OrderApi.BlazorClient.Models.Requests;

/// <summary>[SRP] Represents local login credentials only.</summary>
public sealed record LoginRequest(string Email, string Password);
