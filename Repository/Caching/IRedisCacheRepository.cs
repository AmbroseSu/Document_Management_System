namespace Repository.Caching;

public interface IRedisCacheRepository
{
    Task<T?> GetDataAsync<T>(string key);
    Task SetDataAsync<T>(string key, T data, TimeSpan? expiry = null);
    Task RemoveDataAsync(string key);

}