using System.Net.Http.Json;
using OrderApi.BlazorClient.Models.Requests;
using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.HttpClients;

/// <summary>[Pattern: Template Method][LSP] Order HTTP client that reuses ApiClientBase request pipeline.</summary>
public sealed class OrderApiClient : ApiClientBase, IOrderApiClient
{
    public OrderApiClient(HttpClient http)
        : base(http)
    {
    }

    public Task<ProductCatalogItemResponse[]?> GetAvailableProductsAsync() =>
        ExecuteAsync<ProductCatalogItemResponse[]>(() => Http.GetAsync("/api/orders/products"));

    public Task<OrderResponse[]?> GetMyOrdersAsync() =>
        ExecuteAsync<OrderResponse[]>(() => Http.GetAsync("/api/orders/my"));

    public Task<OrderResponse[]?> GetAllOrdersAsync() =>
        ExecuteAsync<OrderResponse[]>(() => Http.GetAsync("/api/orders"));

    public Task<OrderResponse?> GetOrderByIdAsync(Guid id) =>
        ExecuteAsync<OrderResponse>(() => Http.GetAsync($"/api/orders/{id}"));

    public Task<OrderResponse?> PlaceOrderAsync(PlaceOrderRequest request) =>
        ExecuteAsync<OrderResponse>(() => Http.PostAsJsonAsync("/api/orders", request));

    public async Task CancelOrderAsync(Guid id)
    {
        await ExecuteAsync<object?>(() => Http.DeleteAsync($"/api/orders/{id}/cancel"));
    }
}
