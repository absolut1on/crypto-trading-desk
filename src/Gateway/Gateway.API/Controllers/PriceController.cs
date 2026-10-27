using System.Text.Json;
using Gateway.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace Gateway.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PriceController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly AppDbContext _db;

    public PriceController(IConnectionMultiplexer redis, AppDbContext db)
    {
        _redis = redis;
        _db = db;
    }

    [HttpGet("{pair}")]
    public async Task<IActionResult> GetPrice(string pair)
    {
        var db = _redis.GetDatabase();
        var json = await db.StringGetAsync($"price:{pair.ToUpperInvariant()}");
        if (json.IsNullOrEmpty)
            return NotFound(new { error = $"No price data for {pair}" });

        return Content(json!, "application/json");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPrices()
    {
        var pairs = await _db.CryptoPairs
            .Where(p => p.IsActive)
            .Select(p => p.Symbol)
            .ToListAsync();

        var db = _redis.GetDatabase();
        var results = new List<object>();

        foreach (var symbol in pairs)
        {
            var json = await db.StringGetAsync($"price:{symbol}");
            if (!json.IsNullOrEmpty)
            {
                using var doc = JsonDocument.Parse((string)json!);
                var root = doc.RootElement;
                results.Add(new
                {
                    symbol,
                    bid = root.GetProperty("b").GetString(),
                    ask = root.GetProperty("a").GetString(),
                    last = root.GetProperty("c").GetString(),
                    volume = root.GetProperty("v").GetString(),
                    timestamp = root.GetProperty("E").GetInt64()
                });
            }
        }

        return Ok(results);
    }
}
