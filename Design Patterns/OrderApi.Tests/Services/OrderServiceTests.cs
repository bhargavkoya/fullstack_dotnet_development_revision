using OrderApi.Application.Commands;
using OrderApi.Application.Queries;
using OrderApi.Application.Services;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Domain.Strategies;
using OrderApi.Infrastructure.Factories;
using OrderApi.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace OrderApi.Tests.Services;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task PlaceOrderAsync_UsesDiscountStrategy_AndReturnsOrderDto()
    {
        var orders = new List<Order>();
        var service = CreateService(orders);

        var command = new PlaceOrderCommand(
            "stripe",
            "seasonal",
            [new OrderApi.Application.Commands.OrderItemDto(Guid.NewGuid(), "Widget", 2, 30m)]);

        var response = await service.PlaceOrderAsync(command, Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        Assert.Equal(54m, response.TotalAmount);
        Assert.Single(orders);
        Assert.Equal(OrderStatus.Pending, orders[0].Status);
    }

    [Fact]
    public async Task GetAllOrdersAsync_FiltersForCustomerRole()
    {
        var customerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var otherCustomerId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var orders = new List<Order>
        {
            CreateOrder(customerId, 20m),
            CreateOrder(otherCustomerId, 30m)
        };
        var service = CreateService(orders);

        var result = await service.GetAllOrdersAsync(new GetAllOrdersQuery(customerId, UserRole.Customer));

        Assert.Single(result);
        Assert.Equal(customerId, result.First().CustomerId);
    }

    private static OrderService CreateService(List<Order> orders)
    {
        var provider = new ServiceCollection()
            .AddScoped<StripePaymentProcessor>()
            .AddScoped<PayPalPaymentProcessor>()
            .BuildServiceProvider();

        return new OrderService(
            new FakeOrderReadRepository(orders),
            new FakeOrderWriteRepository(orders),
            new PaymentProcessorFactory(provider),
            new NoDiscountStrategy(),
            new SeasonalDiscountStrategy(),
            new LoyaltyDiscountStrategy());
    }

    private static Order CreateOrder(Guid customerId, decimal amount)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Confirmed,
            TotalAmount = amount,
            CreatedAt = DateTime.UtcNow,
            Items = [new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductName = "Item", Quantity = 1, UnitPrice = amount }]
        };
    }

    private sealed class FakeOrderReadRepository : IOrderReadRepository
    {
        private readonly List<Order> _orders;

        public FakeOrderReadRepository(List<Order> orders) => _orders = orders;

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_orders.FirstOrDefault(order => order.Id == id));

        public Task<IReadOnlyCollection<Order>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Order>>(_orders);
    }

    private sealed class FakeOrderWriteRepository : IOrderWriteRepository
    {
        private readonly List<Order> _orders;

        public FakeOrderWriteRepository(List<Order> orders) => _orders = orders;

        public Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            _orders.Add(order);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DeleteAsync(Order order, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
