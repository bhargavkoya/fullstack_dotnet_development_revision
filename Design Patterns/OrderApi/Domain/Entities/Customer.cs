namespace OrderApi.Domain.Entities;

/// <summary>[SOLID: SRP] Represents a customer profile used for order history and loyalty calculations.</summary>
public class Customer
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int TotalOrdersPlaced { get; set; }
}