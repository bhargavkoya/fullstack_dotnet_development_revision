using OrderApi.Domain.Entities;

namespace OrderApi.Infrastructure.Repositories;

/// <summary>[SOLID: ISP][Pattern: Repository] Exposes only read operations for orders.</summary>
public interface IOrderReadRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default);
}
