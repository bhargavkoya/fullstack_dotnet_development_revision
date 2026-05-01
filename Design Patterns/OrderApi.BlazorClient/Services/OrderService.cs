using OrderApi.BlazorClient.HttpClients;
using OrderApi.BlazorClient.Models.Requests;
using OrderApi.BlazorClient.Models.Responses;
using OrderApi.BlazorClient.State;

namespace OrderApi.BlazorClient.Services;

/// <summary>[Pattern: SRP][DIP][CQRS] Coordinates order reads and writes while keeping application state in sync.</summary>
public sealed class OrderService : IOrderService
{
    private readonly IOrderApiClient _orderApiClient;
    private readonly AppState _appState;

    public OrderService(IOrderApiClient orderApiClient, AppState appState)
    {
        _orderApiClient = orderApiClient;
        _appState = appState;
    }

    /// <inheritdoc />
    public async Task<ProductCatalogItemResponse[]> GetAvailableProductsAsync()
        => await _orderApiClient.GetAvailableProductsAsync() ?? [];

    /// <inheritdoc />
    public async Task<OrderResponse[]> GetMyOrdersAsync()
    {
        var orders = await _orderApiClient.GetMyOrdersAsync() ?? [];
        _appState.SetOrders(orders.ToList());
        return orders;
    }

    /// <inheritdoc />
    public Task<OrderResponse?> GetOrderByIdAsync(Guid id)
        => _orderApiClient.GetOrderByIdAsync(id);

    /// <inheritdoc />
    public async Task<OrderResponse[]> GetAllOrdersAsync()
    {
        var orders = await _orderApiClient.GetAllOrdersAsync() ?? [];
        _appState.SetOrders(orders.ToList());
        return orders;
    }

    /// <inheritdoc />
    public async Task<OrderResponse?> PlaceOrderAsync(PlaceOrderRequest request)
    {
        var createdOrder = await _orderApiClient.PlaceOrderAsync(request);
        if (createdOrder is null)
        {
            return null;
        }

        _appState.AddOrder(createdOrder);
        return createdOrder;
    }

    /// <inheritdoc />
    public async Task CancelOrderAsync(Guid id)
    {
        await _orderApiClient.CancelOrderAsync(id);
        _appState.RemoveOrder(id);
    }
}