using OrderApi.Domain.Entities;

namespace OrderApi.Infrastructure.Repositories;

/// <summary>[SOLID: ISP][Pattern: Repository] Exposes only user persistence operations required by authentication and provisioning flows.</summary>
public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<User?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(User user, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
