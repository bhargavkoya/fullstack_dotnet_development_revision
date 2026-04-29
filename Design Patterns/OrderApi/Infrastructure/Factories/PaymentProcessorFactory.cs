using Microsoft.Extensions.DependencyInjection;

namespace OrderApi.Infrastructure.Factories;

/// <summary>[Pattern: Factory][SOLID: OCP] Resolves a concrete payment processor without exposing callers to implementation details.</summary>
public sealed class PaymentProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentProcessorFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentProcessor Create(string provider)
    {
        return provider.ToLowerInvariant() switch
        {
            "stripe" => _serviceProvider.GetRequiredService<StripePaymentProcessor>(),
            "paypal" => _serviceProvider.GetRequiredService<PayPalPaymentProcessor>(),
            _ => throw new NotSupportedException($"Payment provider '{provider}' is not supported")
        };
    }
}