using OrderApi.Domain.Entities;

namespace OrderApi.Domain.Strategies;

/// <summary>[Pattern: Strategy][SOLID: OCP] Applies a loyalty discount based on the customer's order history.</summary>
public sealed class LoyaltyDiscountStrategy : IDiscountStrategy
{
    public decimal Apply(decimal subtotal, Customer? customer = null)
    {
        if (customer is null)
        {
            return subtotal;
        }

        var discountRate = customer.TotalOrdersPlaced switch
        {
            >= 10 => 0.15m,
            >= 3 => 0.05m,
            _ => 0m
        };

        return subtotal * (1 - discountRate);
    }
}