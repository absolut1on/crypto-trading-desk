using System.Text.Json;
using Gateway.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace Gateway.API.Services;

public class PriceStreamConsumer : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IHubContext<PriceHub> _hub;
    private readonly ILogger<PriceStreamConsumer> _logger;

    public PriceStreamConsumer(IConnectionMultiplexer redis, IHubContext<PriceHub> hub, ILogger<PriceStreamConsumer> logger)
    {
        _redis = redis;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _redis.GetDatabase();
        var lastId = "0-0";

        _logger.LogInformation("PriceStreamConsumer started, reading from stream:prices");

        // Skip to the end of existing entries on startup
        var existing = await db.StreamReadAsync("stream:prices", "0-0", count: 1);
        if (existing.Length > 0)
        {
            var info = await db.StreamInfoAsync("stream:prices");
            lastId = info.LastEntry.Id!;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadAsync("stream:prices", lastId, count: 50);
                if (entries.Length == 0)
                {
                    await Task.Delay(100, stoppingToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    lastId = entry.Id!;
                    var data = entry["data"];
                    if (data.IsNullOrEmpty) continue;

                    using var doc = JsonDocument.Parse(data.ToString());
                    var root = doc.RootElement;

                    if (!root.TryGetProperty("s", out var symbolEl)) continue;
                    var symbol = symbolEl.GetString()!;

                    var ticker = new
                    {
                        symbol,
                        bid = decimal.Parse(root.GetProperty("b").GetString()!),
                        ask = decimal.Parse(root.GetProperty("a").GetString()!),
                        lastPrice = decimal.Parse(root.GetProperty("c").GetString()!),
                        volume = decimal.Parse(root.GetProperty("v").GetString()!),
                        timestamp = DateTimeOffset.FromUnixTimeMilliseconds(root.GetProperty("E").GetInt64()).ToString("o")
                    };

                    await _hub.Clients.Group(symbol).SendAsync("PriceUpdate", ticker, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming price stream");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
