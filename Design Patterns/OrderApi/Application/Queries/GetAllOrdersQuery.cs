using OrderApi.Domain.Enums;

namespace OrderApi.Application.Queries;

/// <summary>[Pattern: CQRS][SOLID: SRP] Represents a request to retrieve orders, scoped by user role and identity.</summary>
public sealed record GetAllOrdersQuery(Guid RequestingUserId, UserRole RequestingUserRole);
