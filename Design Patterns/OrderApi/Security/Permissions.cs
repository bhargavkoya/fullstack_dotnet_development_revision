namespace OrderApi.Security;

/// <summary>[SOLID: SRP] Defines authorization policy name constants used throughout the application.</summary>
public static class Permissions
{
    public const string AdminOnly = "AdminOnly";

    public const string OrderOwnerOrAdmin = "OrderOwnerOrAdmin";
}
