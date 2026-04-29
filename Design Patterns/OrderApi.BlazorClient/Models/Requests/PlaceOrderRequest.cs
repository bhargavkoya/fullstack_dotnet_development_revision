namespace OrderApi.BlazorClient.Models.Requests;

/// <summary>[SRP] Represents the order placement payload exactly as required by the API.</summary>
public sealed record PlaceOrderRequest(string PaymentProvider, string DiscountType, List<PlaceOrderItemRequest> Items);

/// <summary>[SRP] Represents a single order item in a placement request.</summary>
public sealed record PlaceOrderItemRequest(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);
