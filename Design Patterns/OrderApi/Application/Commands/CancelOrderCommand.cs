namespace OrderApi.Application.Commands;

/// <summary>[Pattern: CQRS][SOLID: SRP] Represents a request to cancel an existing order.</summary>
public sealed record CancelOrderCommand(Guid OrderId);
