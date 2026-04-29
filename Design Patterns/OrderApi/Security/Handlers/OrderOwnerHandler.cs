using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Security.Policies;

namespace OrderApi.Security.Handlers;

/// <summary>[SOLID: SRP] Handles OrderOwnerRequirement by checking if the user is the order owner or an admin.</summary>
public sealed class OrderOwnerHandler : AuthorizationHandler<OrderOwnerRequirement, Order>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OrderOwnerRequirement requirement, Order? resource)
    {
        if (resource is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleClaim = context.User.FindFirstValue(ClaimTypes.Role);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Allow if user is the order owner or is an admin.
        if (resource.CustomerId == userId || roleClaim == UserRole.Admin.ToString())
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
