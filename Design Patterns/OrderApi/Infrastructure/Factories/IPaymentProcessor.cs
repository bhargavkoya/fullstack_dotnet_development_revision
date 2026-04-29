namespace OrderApi.Infrastructure.Factories;

/// <summary>[Pattern: Factory][SOLID: LSP] Defines a payment processor that can be substituted by any provider implementation.</summary>
public interface IPaymentProcessor
{
    Task<string> ProcessAsync(decimal amount, CancellationToken cancellationToken = default);
}