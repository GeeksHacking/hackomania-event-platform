using SqlSugar;
using SugarRedis;

namespace HackOMania.Api.Services;

public class SqlSugarRedisCache : ICacheService
{
    // SugarRedis client should be singleton
    private static SugarRedisClient? _service;
    private static readonly object _lock = new();

    public SqlSugarRedisCache(string? connectionString = null)
    {
        if (_service == null)
        {
            lock (_lock)
            {
                if (_service == null)
                {
                    // Initialize SugarRedis with connection string if provided
                    // Default: 127.0.0.1:6379,password=,connectTimeout=3000,connectRetry=1,syncTimeout=10000,DefaultDatabase=0
                    _service = string.IsNullOrWhiteSpace(connectionString)
                        ? new SugarRedisClient()
                        : new SugarRedisClient(connectionString);
                }
            }
        }
    }

    public void Add<TV>(string key, TV value)
    {
        if (value != null)
        {
            _service!.Set(key, value);
        }
    }

    public void Add<TV>(string key, TV value, int cacheDurationInSeconds)
    {
        if (value != null)
        {
            _service!.SetBySeconds(key, value, cacheDurationInSeconds);
        }
    }

    public bool ContainsKey<TV>(string key)
    {
        return _service!.Exists(key);
    }

    public TV Get<TV>(string key)
    {
        return _service!.Get<TV>(key);
    }

    public IEnumerable<string> GetAllKey<TV>()
    {
        // Only query keys used by SqlSugar for performance
        return _service!.SearchCacheRegex("SqlSugarDataCache.*");
    }

    public TV GetOrCreate<TV>(string cacheKey, Func<TV> create, int cacheDurationInSeconds = int.MaxValue)
    {
        if (this.ContainsKey<TV>(cacheKey))
        {
            var result = this.Get<TV>(cacheKey);
            if (result == null)
            {
                return create();
            }
            else
            {
                return result;
            }
        }
        else
        {
            var result = create();
            this.Add(cacheKey, result, cacheDurationInSeconds);
            return result;
        }
    }

    public void Remove<TV>(string key)
    {
        _service!.Remove(key);
    }
}
