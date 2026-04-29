using OrderApi.Domain.Entities;

namespace OrderApi.Infrastructure.Repositories;

/// <summary>[SOLID: ISP][Pattern: Repository] Exposes only write operations for orders.</summary>
public interface IOrderWriteRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

    Task DeleteAsync(Order order, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
