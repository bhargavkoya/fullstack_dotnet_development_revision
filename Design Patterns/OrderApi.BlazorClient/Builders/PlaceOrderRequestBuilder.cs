using OrderApi.BlazorClient.Models.Requests;

namespace OrderApi.BlazorClient.Builders;

/// <summary>[Pattern: Builder][SRP] Fluently constructs a validated place-order request.</summary>
public sealed class PlaceOrderRequestBuilder
{
    private string _paymentProvider = string.Empty;
    private string _discountType = "none";
    private readonly List<PlaceOrderItemRequest> _items = [];

    /// <summary>[Pattern: Builder] Sets the payment provider used by the order request.</summary>
    public PlaceOrderRequestBuilder WithPaymentProvider(string paymentProvider)
    {
        _paymentProvider = ValidateRequired(paymentProvider, nameof(paymentProvider));
        return this;
    }

    /// <summary>[Pattern: Builder] Sets the discount type used by the order request.</summary>
    public PlaceOrderRequestBuilder WithDiscountType(string discountType)
    {
        _discountType = ValidateRequired(discountType, nameof(discountType));
        return this;
    }

    /// <summary>[Pattern: Builder] Adds a validated order item to the request.</summary>
    public PlaceOrderRequestBuilder AddItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product ID is required.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            throw new ArgumentException("Product name is required.", nameof(productName));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (unitPrice <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price must be greater than zero.");
        }

        _items.Add(new PlaceOrderItemRequest(productId, productName.Trim(), quantity, unitPrice));
        return this;
    }

    /// <summary>[Pattern: Builder] Creates the final validated request payload.</summary>
    public PlaceOrderRequest Build()
    {
        if (string.IsNullOrWhiteSpace(_paymentProvider))
        {
            throw new InvalidOperationException("Payment provider is required.");
        }

        if (string.IsNullOrWhiteSpace(_discountType))
        {
            throw new InvalidOperationException("Discount type is required.");
        }

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("At least one order item is required.");
        }

        return new PlaceOrderRequest(_paymentProvider.Trim(), _discountType.Trim(), [.. _items]);
    }

    private static string ValidateRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value;
    }
}