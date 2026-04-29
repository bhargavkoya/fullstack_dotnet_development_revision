namespace OrderApi.Infrastructure.Factories;

/// <summary>[Pattern: Factory][SOLID: LSP] Processes payments through the PayPal provider.</summary>
public sealed class PayPalPaymentProcessor : IPaymentProcessor
{
    public Task<string> ProcessAsync(decimal amount, CancellationToken cancellationToken = default)
    {
        var paymentReference = $"paypal_{Guid.NewGuid():N}";

        return Task.FromResult(paymentReference);
    }
}