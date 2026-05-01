namespace OrderApi.Application.Dtos;

/// <summary>[SRP] Represents a product that can be selected while placing an order.</summary>
public sealed record ProductCatalogItemDto(Guid ProductId, string ProductName, decimal UnitPrice);
