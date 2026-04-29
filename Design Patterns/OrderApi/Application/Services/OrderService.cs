using OrderApi.Application.Commands;
using OrderApi.Application.Dtos;
using OrderApi.Application.Queries;
using OrderApi.Builders;
using OrderApi.Domain.Entities;
using OrderApi.Domain.Enums;
using OrderApi.Domain.Events;
using OrderApi.Domain.Strategies;
using OrderApi.Infrastructure.Factories;
using OrderApi.Infrastructure.Repositories;

namespace OrderApi.Application.Services;

/// <summary>[SOLID: SRP][SOLID: DIP][Pattern: Strategy][Pattern: Factory] Handles order placement, cancellation, and queries with discount/payment integration.</summary>
public sealed class OrderService : IOrderService
{
    private readonly IOrderReadRepository _readRepository;
    private readonly IOrderWriteRepository _writeRepository;
    private readonly PaymentProcessorFactory _paymentProcessorFactory;
    private readonly NoDiscountStrategy _noDiscountStrategy;
    private readonly SeasonalDiscountStrategy _seasonalDiscountStrategy;
    private readonly LoyaltyDiscountStrategy _loyaltyDiscountStrategy;

    public OrderService(
        IOrderReadRepository readRepository,
        IOrderWriteRepository writeRepository,
        PaymentProcessorFactory paymentProcessorFactory,
        NoDiscountStrategy noDiscountStrategy,
        SeasonalDiscountStrategy seasonalDiscountStrategy,
        LoyaltyDiscountStrategy loyaltyDiscountStrategy)
    {
        _readRepository = readRepository;
        _writeRepository = writeRepository;
        _paymentProcessorFactory = paymentProcessorFactory;
        _noDiscountStrategy = noDiscountStrategy;
        _seasonalDiscountStrategy = seasonalDiscountStrategy;
        _loyaltyDiscountStrategy = loyaltyDiscountStrategy;
    }

    public async Task<OrderDto> PlaceOrderAsync(PlaceOrderCommand command, Guid customerId, CancellationToken cancellationToken = default)
    {
        // [Pattern: Strategy] Select discount strategy based on command
        var discountStrategy = SelectDiscountStrategy(command.DiscountType);

        // Convert command items to domain OrderItem objects
        var orderItems = command.Items
            .Select(dto => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = dto.ProductId,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice
            })
            .ToList();

        // Calculate subtotal
        var subtotal = orderItems.Sum(item => item.LineTotal);

        // Apply discount strategy
        var discountedTotal = discountStrategy.Apply(subtotal);

        // [Pattern: Factory] Select payment processor by provider
        var paymentProcessor = _paymentProcessorFactory.Create(command.PaymentProvider);

        // Process payment (returns a payment reference)
        var paymentReference = await paymentProcessor.ProcessAsync(discountedTotal, cancellationToken).ConfigureAwait(false);

        // Build the order using [Pattern: Builder]
        var order = new OrderBuilder()
            .WithId(Guid.NewGuid())
            .WithCustomerId(customerId)
            .AddItems(orderItems)
            .WithStatus(OrderStatus.Pending)
            .WithTotalAmount(discountedTotal)
            .Build();

        // [Pattern: Observer] Raise domain event
        order.AddDomainEvent(new OrderPlacedEvent(order.Id, customerId, discountedTotal, DateTime.UtcNow));

        // Persist the order
        await _writeRepository.AddAsync(order, cancellationToken).ConfigureAwait(false);
        await _writeRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // Map to DTO for response
        return MapOrderToDto(order);
    }

    public async Task CancelOrderAsync(Guid orderId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        // Retrieve the order
        var order = await _readRepository.GetByIdAsync(orderId, cancellationToken).ConfigureAwait(false);

        if (order is null)
        {
            throw new KeyNotFoundException($"Order {orderId} not found.");
        }

        // [Pattern: CQRS + Authorization] Resource-based check: only order owner or admin can cancel
        // (admin check is handled at controller level; this enforces business rule)
        if (order.CustomerId != requestingUserId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own orders.");
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException("Order is already cancelled.");
        }

        // Update status
        order.Status = OrderStatus.Cancelled;

        // [Pattern: Observer] Raise domain event
        order.AddDomainEvent(new OrderCancelledEvent(order.Id, order.CustomerId, DateTime.UtcNow));

        // Persist changes
        await _writeRepository.UpdateAsync(order, cancellationToken).ConfigureAwait(false);
        await _writeRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, CancellationToken cancellationToken = default)
    {
        // Retrieve the order
        var order = await _readRepository.GetByIdAsync(orderId, cancellationToken).ConfigureAwait(false);

        if (order is null)
        {
            return null;
        }

        // [Pattern: CQRS + Authorization] Resource-based check: only order owner or admin can view
        // (admin bypass is handled at controller level; this enforces business rule)
        if (order.CustomerId != requestingUserId)
        {
            throw new UnauthorizedAccessException("You can only view your own orders.");
        }

        return MapOrderToDto(order);
    }

    public async Task<IReadOnlyCollection<OrderDto>> GetAllOrdersAsync(GetAllOrdersQuery query, CancellationToken cancellationToken = default)
    {
        // [Pattern: CQRS] Query with role-based filtering
        var orders = await _readRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);

        // If customer, filter to their orders only; if admin, return all
        if (query.RequestingUserRole == UserRole.Customer)
        {
            orders = orders
                .Where(order => order.CustomerId == query.RequestingUserId)
                .ToList();
        }

        return orders.Select(MapOrderToDto).ToList();
    }

    private IDiscountStrategy SelectDiscountStrategy(string discountType)
    {
        return discountType.ToLowerInvariant() switch
        {
            "seasonal" => _seasonalDiscountStrategy,
            "loyalty" => _loyaltyDiscountStrategy,
            _ => _noDiscountStrategy
        };
    }

    private static OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.Items
                .Select(item => new Dtos.OrderItemDto(item.Id, item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, item.LineTotal))
                .ToList(),
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt
        );
    }
}
