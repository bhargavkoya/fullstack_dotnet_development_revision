namespace OrderApi.Domain.Events;

/// <summary>[Pattern: Observer][SOLID: SRP] Raised when an order is successfully placed.</summary>
public sealed record OrderPlacedEvent(Guid OrderId, Guid CustomerId, decimal TotalAmount, DateTime OccurredAt) : IDomainEvent;