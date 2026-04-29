namespace OrderApi.BlazorClient.Models.Responses;

/// <summary>[SRP] Represents an order returned by the API.</summary>
public sealed record OrderResponse(
    Guid Id,
    Guid CustomerId,
    List<OrderItemResponse> Items,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt);
