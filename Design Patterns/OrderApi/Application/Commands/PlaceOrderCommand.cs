namespace OrderApi.Application.Commands;

/// <summary>[Pattern: CQRS][SOLID: SRP] Represents a request to place a new order.</summary>
public sealed record PlaceOrderCommand(
    string PaymentProvider,
    string DiscountType,
    List<OrderItemDto> Items
);

/// <summary>Data transfer object for an order item line.</summary>
public sealed record OrderItemDto(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
