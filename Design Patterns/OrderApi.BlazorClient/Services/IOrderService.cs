using OrderApi.BlazorClient.Models.Requests;
using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.Services;

/// <summary>[Pattern: ISP][SRP][DIP][CQRS] Exposes order query and command operations to the UI layer.</summary>
public interface IOrderService
{
    /// <summary>[CQRS] Queries the authenticated user's orders.</summary>
    Task<OrderResponse[]> GetMyOrdersAsync();

    /// <summary>[CQRS] Queries a single order by identifier.</summary>
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);

    /// <summary>[CQRS] Queries all orders for administrators.</summary>
    Task<OrderResponse[]> GetAllOrdersAsync();

    /// <summary>[CQRS] Places a new order and returns the created resource.</summary>
    Task<OrderResponse?> PlaceOrderAsync(PlaceOrderRequest request);

    /// <summary>[CQRS] Cancels an order by identifier.</summary>
    Task CancelOrderAsync(Guid id);
}