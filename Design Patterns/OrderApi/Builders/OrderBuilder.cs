using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;

namespace OrderApi.Builders;

/// <summary>[Pattern: Builder][SOLID: SRP] Provides a fluent API for constructing Order aggregates.</summary>
public sealed class OrderBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _customerId;
    private readonly List<OrderItem> _items = [];
    private OrderStatus _status = OrderStatus.Pending;
    private decimal _totalAmount = 0m;

    public OrderBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public OrderBuilder WithCustomerId(Guid customerId)
    {
        _customerId = customerId;
        return this;
    }

    public OrderBuilder AddItem(OrderItem item)
    {
        _items.Add(item);
        return this;
    }

    public OrderBuilder AddItems(IEnumerable<OrderItem> items)
    {
        _items.AddRange(items);
        return this;
    }

    public OrderBuilder WithStatus(OrderStatus status)
    {
        _status = status;
        return this;
    }

    public OrderBuilder WithTotalAmount(decimal amount)
    {
        _totalAmount = amount;
        return this;
    }

    public Order Build()
    {
        if (_customerId == Guid.Empty)
        {
            throw new InvalidOperationException("CustomerId is required to build an order.");
        }

        // If items are provided but total is not calculated, compute it from line totals.
        var calculatedTotal = _items.Sum(item => item.LineTotal);
        var finalTotal = _totalAmount > 0 ? _totalAmount : calculatedTotal;

        return new Order
        {
            Id = _id,
            CustomerId = _customerId,
            Items = _items,
            Status = _status,
            TotalAmount = finalTotal,
            CreatedAt = DateTime.UtcNow
        };
    }
}
