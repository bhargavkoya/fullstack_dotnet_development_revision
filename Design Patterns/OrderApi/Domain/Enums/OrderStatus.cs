namespace OrderApi.Domain.Enums;

/// <summary>[SOLID: SRP] Represents the lifecycle state of an order.</summary>
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2
}