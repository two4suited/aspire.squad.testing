using StackExchange.Redis;
using System.Text.Json;

namespace DogTeams.Api.Caching;

/// <summary>
/// Generic caching service backed by Redis for reducing Cosmos DB RU consumption.
/// </summary>
public interface IRedisCacheService
{
    /// <summary>Gets a cached value or computes and caches it if missing (supports nullable types).</summary>
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? ttl = null) where T : class;

    /// <summary>Sets a value in the cache.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class;

    /// <summary>Gets a cached value.</summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>Removes a cached value.</summary>
    Task RemoveAsync(string key);

    /// <summary>Removes multiple cached values by pattern.</summary>
    Task RemoveByPatternAsync(string pattern);
}

/// <summary>
/// Redis-backed implementation of the caching service.
/// </summary>
public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private const int DefaultTtlMinutes = 10;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T?>> factory, TimeSpan? ttl = null) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        // Try to get from cache
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        // Cache miss - compute value
        var value = await factory();
        if (value != null)
        {
            await SetAsync(key, value, ttl);
        }

        return value;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        if (value == null)
            throw new ArgumentNullException(nameof(value));

        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(value);
        var expiry = ttl ?? TimeSpan.FromMinutes(DefaultTtlMinutes);

        await db.StringSetAsync(key, json, expiry);
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (!value.HasValue)
            return null;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task RemoveAsync(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));

        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            throw new ArgumentException("Pattern cannot be null or empty.", nameof(pattern));

        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var keys = server.Keys(pattern: pattern);

        foreach (var key in keys)
        {
            await RemoveAsync(key.ToString());
        }
    }
}
