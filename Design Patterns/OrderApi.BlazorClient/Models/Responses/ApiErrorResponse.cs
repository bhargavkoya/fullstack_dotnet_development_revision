namespace OrderApi.BlazorClient.Models.Responses;

/// <summary>[SRP] Represents API error payloads mapped from failed HTTP responses.</summary>
public sealed record ApiErrorResponse(string? Message);
