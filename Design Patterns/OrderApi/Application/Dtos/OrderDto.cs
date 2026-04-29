namespace OrderApi.Application.Dtos;

/// <summary>Data transfer object for order responses.</summary>
public sealed record OrderDto(
    Guid Id,
    Guid CustomerId,
    List<OrderItemDto> Items,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt
);

/// <summary>Data transfer object for order item responses.</summary>
public sealed record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
