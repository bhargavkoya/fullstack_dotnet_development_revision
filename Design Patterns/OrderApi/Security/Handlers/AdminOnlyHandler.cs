using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using OrderApi.Domain.Enums;
using OrderApi.Security.Policies;

namespace OrderApi.Security.Handlers;

/// <summary>[SOLID: SRP] Handles AdminOnlyRequirement by checking the Role claim.</summary>
public sealed class AdminOnlyHandler : AuthorizationHandler<AdminOnlyRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminOnlyRequirement requirement)
    {
        var roleClaim = context.User.FindFirstValue(ClaimTypes.Role);

        if (roleClaim == UserRole.Admin.ToString())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
