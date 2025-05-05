using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Repository.Caching.Impl;

public class RedisCacheRepository : IRedisCacheRepository
{
    private readonly IDistributedCache _cache;

    public RedisCacheRepository(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T>? GetDataAsync<T>(string key)
    {
        var data = await _cache.GetStringAsync(key);
        
        if (data == null)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(data);
    }

    public async Task SetDataAsync<T>(string key, T data, TimeSpan? expiry = null)
    {
        if (expiry is null)
        {
            expiry = TimeSpan.FromDays(1);
        }

        if (expiry.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiry), "Expiry must be a positive timespan.");
        }

        await _cache.SetStringAsync(key, JsonSerializer.Serialize(data), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        });
    }

    public async Task RemoveDataAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}