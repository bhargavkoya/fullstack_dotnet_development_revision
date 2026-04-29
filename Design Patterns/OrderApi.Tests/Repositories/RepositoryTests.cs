using Microsoft.EntityFrameworkCore;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Infrastructure.Persistence;
using OrderApi.Infrastructure.Repositories;

namespace OrderApi.Tests.Repositories;

public sealed class RepositoryTests
{
    [Fact]
    public async Task UserRepository_FindByEmailAsync_ReturnsSeededUser()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);
        var repository = new UserRepository(dbContext);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hash",
            Role = UserRole.Customer
        };

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        var found = await repository.FindByEmailAsync("user@example.com");

        Assert.NotNull(found);
        Assert.Equal(user.Email, found!.Email);
    }

    [Fact]
    public async Task OrderRepository_GetByIdAsync_ReturnsOrderWithItems()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new AppDbContext(options);
        var repository = new OrderRepository(dbContext);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            Status = OrderStatus.Pending,
            TotalAmount = 42m,
            Items = [new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Test", Quantity = 1, UnitPrice = 42m }]
        };

        await repository.AddAsync(order);
        await repository.SaveChangesAsync();

        var found = await repository.GetByIdAsync(order.Id);

        Assert.NotNull(found);
        Assert.Single(found!.Items);
    }
}
