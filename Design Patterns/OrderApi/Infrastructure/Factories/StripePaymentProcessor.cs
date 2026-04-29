namespace OrderApi.Infrastructure.Factories;

/// <summary>[Pattern: Factory][SOLID: LSP] Processes payments through the Stripe provider.</summary>
public sealed class StripePaymentProcessor : IPaymentProcessor
{
    public Task<string> ProcessAsync(decimal amount, CancellationToken cancellationToken = default)
    {
        var paymentReference = $"stripe_{Guid.NewGuid():N}";

        return Task.FromResult(paymentReference);
    }
}