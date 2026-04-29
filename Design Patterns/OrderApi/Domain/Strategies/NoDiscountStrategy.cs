using OrderApi.Domain.Entities;

namespace OrderApi.Domain.Strategies;

/// <summary>[Pattern: Strategy][SOLID: OCP] Returns the order subtotal unchanged.</summary>
public sealed class NoDiscountStrategy : IDiscountStrategy
{
    public decimal Apply(decimal subtotal, Customer? customer = null) => subtotal;
}