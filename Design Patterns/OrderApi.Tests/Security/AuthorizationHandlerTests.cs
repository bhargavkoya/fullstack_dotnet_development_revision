using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Security.Handlers;
using OrderApi.Security.Policies;

namespace OrderApi.Tests.Security;

public sealed class AuthorizationHandlerTests
{
    [Fact]
    public async Task AdminOnlyHandler_Succeeds_ForAdminRole()
    {
        var handler = new AdminOnlyHandler();
        var requirement = new AdminOnlyRequirement();
        var principal = CreatePrincipal(Guid.NewGuid(), UserRole.Admin);
        var context = new AuthorizationHandlerContext([requirement], principal, null);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task OrderOwnerHandler_Succeeds_WhenUserOwnsOrder()
    {
        var userId = Guid.NewGuid();
        var handler = new OrderOwnerHandler();
        var requirement = new OrderOwnerRequirement();
        var principal = CreatePrincipal(userId, UserRole.Customer);
        var order = new Order { Id = Guid.NewGuid(), CustomerId = userId, Status = OrderStatus.Pending };
        var context = new AuthorizationHandlerContext([requirement], principal, order);

        await handler.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId, UserRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }
}
