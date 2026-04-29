namespace OrderApi.Domain.Events;

/// <summary>[Pattern: Observer][SOLID: SRP] Raised when an order is cancelled.</summary>
public sealed record OrderCancelledEvent(Guid OrderId, Guid CustomerId, DateTime OccurredAt) : IDomainEvent;