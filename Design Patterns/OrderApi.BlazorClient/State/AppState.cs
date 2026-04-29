using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.State;

/// <summary>[Pattern: Observer][SRP][DIP] Central observable order state for the Blazor client.</summary>
public sealed class AppState : IAppState
{
    private List<OrderResponse> _orders = new();

    /// <inheritdoc />
    public event Action? OnChange;

    /// <inheritdoc />
    public IReadOnlyList<OrderResponse> Orders => _orders.AsReadOnly();

    /// <summary>[Pattern: Observer] Replaces the current orders and notifies subscribers.</summary>
    public void SetOrders(List<OrderResponse> orders)
    {
        _orders = AppStateReducer.ApplySetOrders(_orders, orders);
        NotifyStateChanged();
    }

    /// <summary>[Pattern: Observer] Adds a new order and notifies subscribers.</summary>
    public void AddOrder(OrderResponse order)
    {
        _orders = AppStateReducer.ApplyAddOrder(_orders, order);
        NotifyStateChanged();
    }

    /// <summary>[Pattern: Observer] Removes an order by identifier and notifies subscribers.</summary>
    public void RemoveOrder(Guid id)
    {
        _orders = AppStateReducer.ApplyRemoveOrder(_orders, id);
        NotifyStateChanged();
    }

    /// <inheritdoc />
    public OrderResponse? GetOrderById(Guid id)
        => _orders.FirstOrDefault(order => order.Id == id);

    private void NotifyStateChanged() => OnChange?.Invoke();
}