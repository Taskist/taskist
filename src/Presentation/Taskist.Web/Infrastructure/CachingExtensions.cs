using Taskist.Core.Caching;
using Taskist.Core.Common;
using ZiggyCreatures.Caching.Fusion;

namespace Taskist.Web.Infrastructure;

public static class CachingExtensions
{
    public static IServiceCollection AddApplicationCaching(this IServiceCollection services)
    {
        services.AddFusionCache()
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(Constant.CacheTime)
            });

        services.AddTransient<ICacheManager, FusionCacheManager>();

        return services;
    }
}
