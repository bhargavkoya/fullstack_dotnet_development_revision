using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using OrderApi.Domain.Entities;
using OrderApi.Infrastructure.Repositories;

namespace OrderApi.Infrastructure.Decorators;

/// <summary>[Pattern: Decorator][SOLID: SRP][SOLID: OCP] Wraps IOrderReadRepository with cache-aside logic scoped to the current user.</summary>
public sealed class CachedOrderRepository : IOrderReadRepository
{
    private readonly OrderRepository _inner;
    private readonly IMemoryCache _cache;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const int CacheDurationMinutes = 5;

    public CachedOrderRepository(OrderRepository inner, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
    {
        _inner = inner;
        _cache = cache;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = ExtractCurrentUserId();
        var cacheKey = $"order_{id}_user_{userId}";

        if (_cache.TryGetValue(cacheKey, out Order? cachedOrder))
        {
            return cachedOrder;
        }

        var order = await _inner.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (order is not null)
        {
            _cache.Set(cacheKey, order, TimeSpan.FromMinutes(CacheDurationMinutes));
        }

        return order;
    }

    public async Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var userId = ExtractCurrentUserId();
        var cacheKey = $"order_all_user_{userId}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyCollection<Order>? cachedOrders))
        {
            return cachedOrders;
        }

        var orders = await _inner.GetAllAsync(cancellationToken).ConfigureAwait(false);

        _cache.Set(cacheKey, orders, TimeSpan.FromMinutes(CacheDurationMinutes));

        return orders;
    }

    private string ExtractCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userIdClaim ?? "anonymous";
    }
}
