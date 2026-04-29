using OrderApi.Application.Commands;
using OrderApi.Application.Dtos;
using OrderApi.Application.Queries;

namespace OrderApi.Application.Services;

/// <summary>[SOLID: DIP][SOLID: SRP] Orchestrates order business logic without exposing persistence or payment details.</summary>
public interface IOrderService
{
    Task<OrderDto> PlaceOrderAsync(PlaceOrderCommand command, Guid customerId, CancellationToken cancellationToken = default);

    Task CancelOrderAsync(Guid orderId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OrderDto>> GetAllOrdersAsync(GetAllOrdersQuery query, CancellationToken cancellationToken = default);
}
