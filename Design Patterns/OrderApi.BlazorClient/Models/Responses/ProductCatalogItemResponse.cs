namespace OrderApi.BlazorClient.Models.Responses;

/// <summary>[SRP] Represents a selectable product option for order placement.</summary>
public sealed record ProductCatalogItemResponse(Guid ProductId, string ProductName, decimal UnitPrice);
