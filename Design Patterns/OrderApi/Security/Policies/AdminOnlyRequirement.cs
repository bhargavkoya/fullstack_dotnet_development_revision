using Microsoft.AspNetCore.Authorization;

namespace OrderApi.Security.Policies;

/// <summary>[SOLID: SRP] Requirement that checks whether the current user has an Admin role.</summary>
public sealed class AdminOnlyRequirement : IAuthorizationRequirement
{
}
