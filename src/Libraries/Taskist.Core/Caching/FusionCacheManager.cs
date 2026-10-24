using System.Collections.Concurrent;
using Taskist.Core.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Taskist.Core.Caching;

public class FusionCacheManager : ICacheManager, ILocker
{
    #region Fields

    private static readonly ConcurrentDictionary<string, byte> _keys = new();
    private readonly IFusionCache _cache;

    #endregion

    #region Ctor

    public FusionCacheManager(IFusionCache cache)
    {
        _cache = cache;
    }

    #endregion

    #region Methods

    public async Task<T> GetAsync<T>(string key, Func<Task<T>> acquire, int? cacheTime = null)
    {
        if (cacheTime <= 0)
            return await acquire();

        _keys.TryAdd(key, 0);

        return await _cache.GetOrSetAsync<T>(key, async _ => await acquire(), BuildOptions(cacheTime));
    }

    public T Get<T>(string key, Func<T> acquire, int? cacheTime = null)
    {
        if (cacheTime <= 0)
            return acquire();

        _keys.TryAdd(key, 0);

        return _cache.GetOrSet<T>(key, _ => acquire(), BuildOptions(cacheTime));
    }

    public async Task SetAsync(string key, object data, int cacheTime)
    {
        if (cacheTime <= 0)
            return;

        _keys.TryAdd(key, 0);

        await _cache.SetAsync(key, data, BuildOptions(cacheTime));
    }

    public async Task<bool> IsSetAsync(string key)
    {
        return (await _cache.TryGetAsync<object>(key)).HasValue;
    }

    public async Task RemoveAsync(string key)
    {
        _keys.TryRemove(key, out _);
        await _cache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        foreach (var key in KeysWithPrefix(prefix))
        {
            _keys.TryRemove(key, out _);
            await _cache.RemoveAsync(key);
        }
    }

    public void RemoveByPrefix(string prefix)
    {
        foreach (var key in KeysWithPrefix(prefix))
        {
            _keys.TryRemove(key, out _);
            _cache.Remove(key);
        }
    }

    public async Task ClearAsync()
    {
        _keys.Clear();
        await _cache.ClearAsync();
    }

    public async Task<bool> PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
    {
        if ((await _cache.TryGetAsync<string>(key)).HasValue)
            return false;

        try
        {
            await _cache.SetAsync(key, key, new FusionCacheEntryOptions { Duration = expirationTime });
            action();

            return true;
        }
        finally
        {
            await _cache.RemoveAsync(key);
        }
    }

    public virtual void Dispose()
    {

    }

    #endregion

    #region Helpers

    private static FusionCacheEntryOptions BuildOptions(int? cacheTime) =>
            new() { Duration = TimeSpan.FromMinutes(cacheTime ?? Constant.CacheTime) };

    private static List<string> KeysWithPrefix(string prefix) =>
            _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

    #endregion
}
