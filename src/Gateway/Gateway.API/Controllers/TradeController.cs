using Gateway.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TradeController : ControllerBase
{
    private readonly AppDbContext _db;

    public TradeController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetTrades([FromQuery] Guid? userId, [FromQuery] string? pair, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var query = _db.Trades.AsQueryable();
        if (userId.HasValue)
            query = query.Where(t => t.UserId == userId.Value);
        if (!string.IsNullOrEmpty(pair))
            query = query.Where(t => t.Pair == pair);

        var trades = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync();

        return Ok(trades);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTrade(Guid id)
    {
        var trade = await _db.Trades.FindAsync(id);
        return trade is null ? NotFound() : Ok(trade);
    }
}
