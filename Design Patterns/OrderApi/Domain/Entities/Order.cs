using OrderApi.Domain.Enums;
using OrderApi.Domain.Events;

namespace OrderApi.Domain.Entities;

/// <summary>[SOLID: SRP] Represents an order aggregate root and its core persistence state.</summary>
public class Order
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public List<OrderItem> Items { get; set; } = [];

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public decimal TotalAmount { get; set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}