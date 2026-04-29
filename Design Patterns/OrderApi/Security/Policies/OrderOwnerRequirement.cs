using Microsoft.AspNetCore.Authorization;
using OrderApi.Domain.Entities;

namespace OrderApi.Security.Policies;

/// <summary>[SOLID: SRP] Requirement that checks whether the current user owns a resource or has an Admin role.</summary>
public sealed class OrderOwnerRequirement : IAuthorizationRequirement
{
}
