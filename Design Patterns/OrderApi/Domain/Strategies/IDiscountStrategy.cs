using OrderApi.Domain.Entities;

namespace OrderApi.Domain.Strategies;

/// <summary>[Pattern: Strategy][SOLID: OCP] Defines a runtime-swappable discount algorithm.</summary>
public interface IDiscountStrategy
{
    decimal Apply(decimal subtotal, Customer? customer = null);
}