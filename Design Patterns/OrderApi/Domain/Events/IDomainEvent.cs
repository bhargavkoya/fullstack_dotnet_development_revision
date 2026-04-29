namespace OrderApi.Domain.Events;

/// <summary>[Pattern: Observer][SOLID: SRP] Marks a domain event raised by an aggregate root.</summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}