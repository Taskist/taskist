using EasyCaching.Core;
using Taskist.Core.Common;

namespace Taskist.Core.Caching;

public class MemoryCacheManager : ICacheManager, ILocker
{
    #region Fields

    private readonly IEasyCachingProvider _provider;

    #endregion

    #region Ctor

    public MemoryCacheManager(IEasyCachingProvider provider)
    {
        _provider = provider;
    }

    #endregion

    #region Methods

    public async Task<T> GetAsync<T>(string key, Func<Task<T>> acquire, int? cacheTime = null)
    {
        if (cacheTime <= 0)
            return await acquire();

        var t = await _provider.GetAsync(key, acquire, TimeSpan.FromMinutes(cacheTime ?? Constant.CacheTime));
        return t.Value;
    }

    public T Get<T>(string key, Func<T> acquire, int? cacheTime = null)
    {
        if (cacheTime <= 0)
            return acquire();

        var t = _provider.Get(key, acquire, TimeSpan.FromMinutes(cacheTime ?? Constant.CacheTime));

        return t.Value;
    }

    public async Task SetAsync(string key, object data, int cacheTime)
    {
        if (cacheTime <= 0)
            return;

        await _provider.SetAsync(key, data, TimeSpan.FromMinutes(cacheTime));
    }

    public async Task<bool> IsSetAsync(string key)
    {
        return await _provider.ExistsAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _provider.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        await _provider.RemoveByPrefixAsync(prefix);
    }

    public void RemoveByPrefix(string prefix)
    {
        _provider.RemoveByPrefix(prefix);
    }

    public async Task ClearAsync()
    {
        await _provider.FlushAsync();
    }

    public virtual void Dispose()
    {
        //nothing special
    }

    public async Task<bool> PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
    {
        if (await _provider.ExistsAsync(key))
            return false;

        try
        {
            await _provider.SetAsync(key, key, expirationTime);
            action();

            return true;
        }
        finally
        {
            await _provider.RemoveAsync(key);
        }
    }

    #endregion
}