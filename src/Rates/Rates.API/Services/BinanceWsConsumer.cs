using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using StackExchange.Redis;

namespace Rates.API.Services;

public class BinanceWsConsumer : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _config;
    private readonly ILogger<BinanceWsConsumer> _logger;

    public BinanceWsConsumer(IConnectionMultiplexer redis, IConfiguration config, ILogger<BinanceWsConsumer> logger)
    {
        _redis = redis;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pairs = _config.GetSection("Binance:Pairs").Get<string[]>() ?? ["btcusdt", "ethusdt"];
        var streams = string.Join("/", pairs.Select(p => $"{p}@ticker"));
        var url = $"wss://stream.binance.com:9443/ws/{streams}";

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConnectAndConsume(url, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "WebSocket connection lost, reconnecting in 5s...");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ConnectAndConsume(string url, CancellationToken ct)
    {
        using var ws = new ClientWebSocket();
        _logger.LogInformation("Connecting to Binance WebSocket: {Url}", url);
        await ws.ConnectAsync(new Uri(url), ct);
        _logger.LogInformation("Connected to Binance WebSocket");

        var buffer = new byte[4096];
        var db = _redis.GetDatabase();

        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
        {
            var result = await ws.ReceiveAsync(buffer, ct);
            if (result.MessageType == WebSocketMessageType.Close) break;

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var ticker = JsonDocument.Parse(json);

            if (ticker.RootElement.TryGetProperty("s", out var symbol))
            {
                var pair = symbol.GetString()!;
                var cacheKey = $"price:{pair}";

                // Cache latest price
                await db.StringSetAsync(cacheKey, json, TimeSpan.FromMinutes(5));

                // Publish to Redis Stream for Gateway to consume
                await db.StreamAddAsync("stream:prices", [new NameValueEntry("data", json)], maxLength: 10000);
            }
        }
    }
}
