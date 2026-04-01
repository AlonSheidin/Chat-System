using StackExchange.Redis;

namespace ChatSystem.Infrastructure.Services;

/// <summary>
/// Low-level wrapper for Redis operations using StackExchange.Redis.
/// </summary>
public interface IRedisService
{
    IDatabase GetDatabase();
    IServer GetServer();

    /// <summary>
    /// Sets a string value with an optional expiration.
    /// </summary>
    Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null);

    /// <summary>
    /// Gets a string value by key.
    /// </summary>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Deletes a key from Redis.
    /// </summary>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// Appends a value to a list (RPUSH).
    /// </summary>
    Task<long> ListAppendAsync(string key, string value);

    /// <summary>
    /// Retrieves elements from a list.
    /// </summary>
    Task<IEnumerable<string>> ListGetAsync(string key, int start = 0, int stop = -1);

    /// <summary>
    /// Trims a list to the specified range (LTRIM).
    /// </summary>
    Task ListTrimAsync(string key, int start, int stop);
}

public class RedisService : IRedisService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public IDatabase GetDatabase() => _redis.GetDatabase();

    public IServer GetServer()
    {
        var endpoints = _redis.GetEndPoints();
        return _redis.GetServer(endpoints[0]);
    }

    public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await GetDatabase().StringSetAsync(key, value, expiry, When.Always, CommandFlags.None);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await GetDatabase().StringGetAsync(key);
    }

    public async Task<bool> RemoveAsync(string key)
    {
        return await GetDatabase().KeyDeleteAsync(key);
    }

    public async Task<long> ListAppendAsync(string key, string value)
    {
        return await GetDatabase().ListRightPushAsync(key, value);
    }

    public async Task<IEnumerable<string>> ListGetAsync(string key, int start = 0, int stop = -1)
    {
        var values = await GetDatabase().ListRangeAsync(key, start, stop);
        return values.Select(v => v.ToString());
    }

    public async Task ListTrimAsync(string key, int start, int stop)
    {
        await GetDatabase().ListTrimAsync(key, start, stop);
    }
}
