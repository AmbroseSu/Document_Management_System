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

    public T? GetData<T>(string key)
    {
        var data = _cache.GetString(key);
        if (data == null)
        {
            return default;
        }
        return JsonSerializer.Deserialize<T>(data);
    }

    public void SetData<T>(string key, T data, TimeSpan? expiry = null)
    {
        if (expiry is null)
        {
            expiry = TimeSpan.FromDays(1);
        }

        if (expiry.Value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(expiry), "Expiry must be a positive timespan.");
        }

        _cache.SetString(key, JsonSerializer.Serialize(data), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        });
    }

    public void RemoveData(string key)
    {
        _cache.Remove(key);
    }
}