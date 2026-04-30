namespace OrderApi.BlazorClient.Models;

/// <summary>[SRP] Represents a client-side draft line item used while composing a place-order request.</summary>
public sealed class PlaceOrderLineDraft
{
    /// <summary>[SRP] Stable draft identifier used by the UI when mutating the line list.</summary>
    public Guid ProductId { get; init; } = Guid.NewGuid();

    /// <summary>[SRP] Product name entered by the user.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>[SRP] Quantity entered by the user.</summary>
    public int Quantity { get; set; } = 1;

    /// <summary>[SRP] Unit price entered by the user.</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>[SRP] Computed line total for the draft row.</summary>
    public decimal LineTotal => Quantity * UnitPrice;
}