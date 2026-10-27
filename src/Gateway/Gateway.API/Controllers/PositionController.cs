using System.Text.Json;
using Gateway.API.Data;
using Gateway.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PositionController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConnectionMultiplexer _redis;

    public PositionController(AppDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis;
    }

    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetPositions(Guid userId)
    {
        var trades = await _db.Trades
            .Where(t => t.UserId == userId && t.Status == TradeStatus.Settled)
            .ToListAsync();

        var grouped = trades.GroupBy(t => t.Pair).Select(g =>
        {
            var netQty = g.Sum(t => t.Side == OrderSide.Buy ? t.Quantity : -t.Quantity);
            var buyCost = g.Where(t => t.Side == OrderSide.Buy).Sum(t => t.Quantity * t.Price);
            var buyQty = g.Where(t => t.Side == OrderSide.Buy).Sum(t => t.Quantity);
            var avgEntry = buyQty > 0 ? buyCost / buyQty : 0m;
            return new { Pair = g.Key, NetQuantity = netQty, AvgEntryPrice = avgEntry, TradeCount = g.Count() };
        }).Where(p => p.NetQuantity != 0).ToList();

        var db = _redis.GetDatabase();
        var results = new List<object>();
        foreach (var pos in grouped)
        {
            var marketPrice = 0m;
            var json = await db.StringGetAsync($"price:{pos.Pair}");
            if (!json.IsNullOrEmpty)
            {
                using var doc = JsonDocument.Parse((string)json!);
                marketPrice = decimal.Parse(doc.RootElement.GetProperty("c").GetString()!);
            }

            var unrealizedPnl = pos.NetQuantity * (marketPrice - pos.AvgEntryPrice);
            results.Add(new
            {
                pos.Pair,
                pos.NetQuantity,
                pos.AvgEntryPrice,
                MarketPrice = marketPrice,
                UnrealizedPnl = unrealizedPnl,
                pos.TradeCount
            });
        }

        return Ok(results);
    }
}
