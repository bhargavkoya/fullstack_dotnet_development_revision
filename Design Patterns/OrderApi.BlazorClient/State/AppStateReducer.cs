using OrderApi.BlazorClient.Models.Responses;

namespace OrderApi.BlazorClient.State;

/// <summary>[Pattern: Strategy][SRP] Provides pure order-state transformation strategies.</summary>
public static class AppStateReducer
{
    /// <summary>[Pattern: Strategy] Replaces the current order list and sorts by newest first.</summary>
    public static List<OrderResponse> ApplySetOrders(List<OrderResponse> _, List<OrderResponse> orders)
        => orders.OrderByDescending(order => order.CreatedAt).ToList();

    /// <summary>[Pattern: Strategy] Removes the order matching the supplied identifier.</summary>
    public static List<OrderResponse> ApplyRemoveOrder(List<OrderResponse> current, Guid id)
        => current.Where(order => order.Id != id).ToList();

    /// <summary>[Pattern: Strategy] Prepends a newly created order to the current list.</summary>
    public static List<OrderResponse> ApplyAddOrder(List<OrderResponse> current, OrderResponse order)
        => [order, .. current];
}