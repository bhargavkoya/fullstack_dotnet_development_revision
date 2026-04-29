using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.Commands;
using OrderApi.Application.Dtos;
using OrderApi.Application.Queries;
using OrderApi.Application.Services;
using OrderApi.Domain.Enums;
using OrderApi.Security;

namespace OrderApi.Controllers;

/// <summary>[SOLID: SRP] Handles order endpoints: placement, cancellation, and retrieval with authorization enforcement.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Place a new order.</summary>
    /// <param name="command">Order placement command with items, discount type, and payment provider.</param>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<OrderDto>> PlaceOrderAsync([FromBody] PlaceOrderCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var customerId = ExtractUserIdFromClaims();
            var order = await _orderService.PlaceOrderAsync(command, customerId, cancellationToken).ConfigureAwait(false);

            return CreatedAtAction(nameof(GetOrderByIdAsync), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>Cancel an order by ID. Only the order owner or an admin can cancel.</summary>
    /// <param name="id">Order ID to cancel.</param>
    [HttpDelete("{id}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelOrderAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var requestingUserId = ExtractUserIdFromClaims();
            var userRole = ExtractUserRoleFromClaims();

            // For admins, allow; for customers, enforce in service layer
            await _orderService.CancelOrderAsync(id, requestingUserId, cancellationToken).ConfigureAwait(false);

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get a single order by ID. Only the order owner or an admin can view.</summary>
    /// <param name="id">Order ID to retrieve.</param>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var requestingUserId = ExtractUserIdFromClaims();
            var order = await _orderService.GetOrderByIdAsync(id, requestingUserId, cancellationToken).ConfigureAwait(false);

            if (order is null)
            {
                return NotFound();
            }

            return Ok(order);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
    }

    /// <summary>Get all orders. Admins see all orders; customers see only their own.</summary>
    [HttpGet]
    [Authorize(Policy = Permissions.AdminOnly)]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetAllOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var requestingUserId = ExtractUserIdFromClaims();
            var userRole = ExtractUserRoleFromClaims();

            var query = new GetAllOrdersQuery(requestingUserId, userRole);
            var orders = await _orderService.GetAllOrdersAsync(query, cancellationToken).ConfigureAwait(false);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    /// <summary>Get all orders for the current authenticated user.</summary>
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyCollection<OrderDto>>> GetMyOrdersAsync(CancellationToken cancellationToken)
    {
        try
        {
            var requestingUserId = ExtractUserIdFromClaims();
            var userRole = ExtractUserRoleFromClaims();

            // For non-admins, force customer role filtering
            var effectiveRole = userRole == UserRole.Admin ? UserRole.Admin : UserRole.Customer;

            var query = new GetAllOrdersQuery(requestingUserId, effectiveRole);
            var orders = await _orderService.GetAllOrdersAsync(query, cancellationToken).ConfigureAwait(false);

            return Ok(orders);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    private Guid ExtractUserIdFromClaims()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID claim is missing or invalid.");
        }

        return userId;
    }

    private UserRole ExtractUserRoleFromClaims()
    {
        var roleClaim = User.FindFirstValue(ClaimTypes.Role);

        return roleClaim switch
        {
            nameof(UserRole.Admin) => UserRole.Admin,
            nameof(UserRole.Customer) => UserRole.Customer,
            _ => UserRole.Customer
        };
    }
}
