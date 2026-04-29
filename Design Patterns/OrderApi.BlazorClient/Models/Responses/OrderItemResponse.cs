namespace OrderApi.BlazorClient.Models.Responses;

/// <summary>[SRP] Represents a single order item returned by the API.</summary>
public sealed record OrderItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);
