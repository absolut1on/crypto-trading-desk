using Gateway.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionController : ControllerBase
{
    private readonly AppDbContext _db;

    public PositionController(AppDbContext db) => _db = db;

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetPositions(Guid userId)
    {
        // Will read from portfolio_positions materialized view in Phase 3
        var trades = await _db.Trades
            .Where(t => t.UserId == userId && t.Status == Entities.TradeStatus.Settled)
            .GroupBy(t => t.Pair)
            .Select(g => new
            {
                Pair = g.Key,
                NetQuantity = g.Sum(t => t.Side == Entities.OrderSide.Buy ? t.Quantity : -t.Quantity),
                TradeCount = g.Count()
            })
            .ToListAsync();

        return Ok(trades);
    }
}
