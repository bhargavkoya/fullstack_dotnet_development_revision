namespace OrderApi.Application.Queries;

/// <summary>[Pattern: CQRS][SOLID: SRP] Represents a request to retrieve a single order by ID.</summary>
public sealed record GetOrderByIdQuery(Guid OrderId);
