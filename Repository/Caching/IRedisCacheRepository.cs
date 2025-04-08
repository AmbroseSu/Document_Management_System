namespace Repository.Caching;

public interface IRedisCacheRepository
{
    T? GetData<T>(string key);
    void SetData<T>(string key, T data, TimeSpan? expiry = null);
    
}