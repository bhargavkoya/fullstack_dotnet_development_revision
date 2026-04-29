using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.State;

/// <summary>[Pattern: Observer][ISP][SRP] Exposes read-only application order state and change notifications.</summary>
public interface IAppState
{
    /// <summary>[Pattern: Observer] Raised after the order state changes so components can re-render.</summary>
    event Action? OnChange;

    /// <summary>[ISP] Provides the current order list as a read-only view.</summary>
    IReadOnlyList<OrderResponse> Orders { get; }

    /// <summary>[ISP] Retrieves a single order from the current state if it exists.</summary>
    OrderResponse? GetOrderById(Guid id);
}