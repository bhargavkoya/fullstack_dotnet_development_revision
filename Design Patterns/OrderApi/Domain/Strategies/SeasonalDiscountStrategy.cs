using OrderApi.Domain.Entities;

namespace OrderApi.Domain.Strategies;

/// <summary>[Pattern: Strategy][SOLID: OCP] Applies a fixed seasonal percentage discount.</summary>
public sealed class SeasonalDiscountStrategy : IDiscountStrategy
{
    private const decimal DiscountRate = 0.10m;

    public decimal Apply(decimal subtotal, Customer? customer = null) => subtotal * (1 - DiscountRate);
}