using OrderApi.BlazorClient.Builders;

namespace OrderApi.BlazorClient.Factories;

/// <summary>[Pattern: Factory][SRP] Creates preconfigured order request builders with sensible defaults.</summary>
public sealed class OrderRequestBuilderFactory
{
    /// <summary>[Pattern: Factory] Creates a builder with the provided discount type and default Stripe payment provider.</summary>
    public PlaceOrderRequestBuilder CreateForCustomer(string discountType)
        => new PlaceOrderRequestBuilder()
            .WithDiscountType(discountType)
            .WithPaymentProvider("stripe");
}