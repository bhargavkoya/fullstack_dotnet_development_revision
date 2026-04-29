using OrderApi.BlazorClient.Models.Requests;
using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.HttpClients;

/// <summary>[ISP][OCP] Exposes only order-related HTTP calls for read/write order workflows.</summary>
public interface IOrderApiClient
{
    Task<OrderResponse[]?> GetMyOrdersAsync();
    Task<OrderResponse[]?> GetAllOrdersAsync();
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<OrderResponse?> PlaceOrderAsync(PlaceOrderRequest request);
    Task CancelOrderAsync(Guid id);
}
