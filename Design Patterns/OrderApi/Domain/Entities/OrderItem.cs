namespace OrderApi.Domain.Entities;

/// <summary>[SOLID: SRP] Represents a single product line within an order.</summary>
public class OrderItem
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal => Quantity * UnitPrice;
}