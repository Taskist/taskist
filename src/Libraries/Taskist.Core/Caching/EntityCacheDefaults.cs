using Taskist.Core.Domain.Common;

namespace Taskist.Core.Caching;

public class EntityCacheDefaults<TEntity> where TEntity : BaseEntity
{
    public static string EntityTypeName => typeof(TEntity).Name.ToLowerInvariant();

    public static string Prefix => $"backlog.{EntityTypeName}.";

    public static string ByIdPrefix => $"backlog.{EntityTypeName}.byid.";

    public static string ByIdsPrefix => $"backlog.{EntityTypeName}.byids.";

    public static string AllPrefix => $"backlog.{EntityTypeName}.all.";
}
