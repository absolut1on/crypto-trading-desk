using Gateway.API.Data;
using Gateway.API.Entities;
using Gateway.API.Sagas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TradeSaga _saga;

    public OrderController(AppDbContext db, TradeSaga saga)
    {
        _db = db;
        _saga = saga;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] Guid? userId, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query = _db.Orders.AsQueryable();
        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var order = await _db.Orders.FindAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Pair = request.Pair,
            Side = request.Side,
            Type = request.Type,
            Quantity = request.Quantity,
            Price = request.Price
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var (result, error) = await _saga.ExecuteAsync(order);
        if (error is not null)
            return UnprocessableEntity(new { order = result, error });

        return CreatedAtAction(nameof(GetOrder), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order is null) return NotFound();
        if (!StateMachines.OrderStateMachine.CanTransition(order.Status, "Cancel"))
            return BadRequest(new { error = $"Cannot cancel order in {order.Status} state" });

        order.Status = StateMachines.OrderStateMachine.Transition(order.Status, "Cancel");
        order.UpdatedAt = DateTime.UtcNow;
        _db.OrderEvents.Add(new OrderEvent
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            EventType = "Cancel"
        });
        await _db.SaveChangesAsync();
        return Ok(order);
    }
}

public record PlaceOrderRequest(Guid UserId, string Pair, OrderSide Side, OrderType Type, decimal Quantity, decimal? Price);
