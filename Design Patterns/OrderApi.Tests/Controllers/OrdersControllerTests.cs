using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.Commands;
using OrderApi.Application.Dtos;
using OrderApi.Application.Queries;
using OrderApi.Application.Services;
using OrderApi.Controllers;
using OrderApi.Domain.Enums;

namespace OrderApi.Tests.Controllers;

public sealed class OrdersControllerTests
{
    [Fact]
    public async Task PlaceOrderAsync_ReturnsCreated_WithOrderDto()
    {
        var orderService = new FakeOrderService();
        var controller = new OrdersController(orderService);
        controller.ControllerContext = CreateControllerContext(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), UserRole.Customer);

        var command = new PlaceOrderCommand(
            "stripe",
            "none",
            [new OrderApi.Application.Commands.OrderItemDto(Guid.NewGuid(), "Product A", 2, 25m)]);

        var result = await controller.PlaceOrderAsync(command, CancellationToken.None);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var order = Assert.IsType<OrderDto>(createdResult.Value);
        Assert.Equal(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), order.Id);
        Assert.Equal(50m, order.TotalAmount);
    }

    private static ControllerContext CreateControllerContext(Guid userId, UserRole role)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        };

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };
    }

    private sealed class FakeOrderService : IOrderService
    {
        public Task<OrderDto> PlaceOrderAsync(PlaceOrderCommand command, Guid customerId, CancellationToken cancellationToken = default)
            => Task.FromResult(new OrderDto(
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                customerId,
                [new OrderApi.Application.Dtos.OrderItemDto(Guid.NewGuid(), Guid.NewGuid(), "Product A", 2, 25m, 50m)],
                "Pending",
                50m,
                DateTime.UtcNow));

        public Task CancelOrderAsync(Guid orderId, Guid requestingUserId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, CancellationToken cancellationToken = default)
            => Task.FromResult<OrderDto?>(null);

        public Task<IReadOnlyCollection<OrderDto>> GetAllOrdersAsync(GetAllOrdersQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<OrderDto>>([]);
    }
}
