using StackExchange.Redis;
using System.Text.Json;

namespace Gateway.API.Services;

public class RedisStreamService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisStreamService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task PublishAsync<T>(string stream, T message)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(message);
        await db.StreamAddAsync(stream, [new NameValueEntry("data", json)]);
    }

    public async Task<string?> GetLatestAsync(string key)
    {
        var db = _redis.GetDatabase();
        return await db.StringGetAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        var db = _redis.GetDatabase();
        if (expiry.HasValue)
            await db.StringSetAsync(key, value, new Expiration(expiry.Value));
        else
            await db.StringSetAsync(key, value);
    }
}
