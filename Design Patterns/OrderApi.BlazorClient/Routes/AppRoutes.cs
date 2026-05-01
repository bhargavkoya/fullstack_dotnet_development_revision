namespace OrderApi.BlazorClient.Routes;

/// <summary>[SRP] Route constants avoid magic strings in navigation and [Authorize] directives.</summary>
public static class AppRoutes
{
    public const string Login = "/login";
    public const string MyOrders = "/orders/my";
    public const string AllOrders = "/orders/all";
    public const string PlaceOrder = "/orders/place";
    public const string OrderDetail = "/orders/{Id:guid}";
    public const string OAuthCallback = "/oauth-callback";
    public const string Unauthorized = "/unauthorized";

    public static string OrderDetailPath(Guid id) => $"/orders/{id}";
}
