using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Common;

namespace Taskist.Service.Masters;

public class GenericAttributeService : IGenericAttributeService
{
    #region Fields

    protected readonly IRepository<GenericAttribute> _genericAttributeRepository;
    protected readonly ICacheManager _cacheManager;

    #endregion

    #region Ctor

    public GenericAttributeService(IRepository<GenericAttribute> genericAttributeRepository,
        ICacheManager cacheManager)
    {
        _genericAttributeRepository = genericAttributeRepository;
        _cacheManager = cacheManager;
    }

    #endregion

    #region Methods

    public async Task<IList<GenericAttribute>> GetAttributesForEntityAsync(int entityId, string keyGroup)
    {
        var query = from ga in _genericAttributeRepository.Table
                    where ga.EntityId == entityId &&
                          ga.KeyGroup == keyGroup
                    select ga;

        var key = string.Format(ServiceConstant.GenericAttributeCacheKey, entityId, keyGroup);

        return await query.ToListAsync();
    }

    public async Task<TPropType> GetAttributeAsync<TPropType>(BaseEntity entity, string key, TPropType defaultValue = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var keyGroup = entity.GetType().Name;

        var props = await GetAttributesForEntityAsync(entity.Id, keyGroup);

        if (props == null)
            return defaultValue;

        if (!props.Any())
            return defaultValue;

        var prop = props.FirstOrDefault(ga => ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

        if (prop == null || string.IsNullOrEmpty(prop.Value))
            return defaultValue;

        return CommonHelper.To<TPropType>(prop.Value);
    }

    public async Task<TPropType> GetAttributeAsync<TEntity, TPropType>(int entityId, string key, TPropType defaultValue = default) where TEntity : BaseEntity
    {
        var entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
        entity.Id = entityId;

        return await GetAttributeAsync(entity, key, defaultValue);
    }

    public async Task SaveAttributeAsync<TPropType>(BaseEntity entity, string key, TPropType value)
    {
        ArgumentNullException.ThrowIfNull(entity);

        ArgumentNullException.ThrowIfNull(key);

        var keyGroup = entity.GetType().Name;

        var props = (await GetAttributesForEntityAsync(entity.Id, keyGroup))
            .ToList();
        var prop = props.FirstOrDefault(ga => ga.Key.Equals(key, StringComparison.InvariantCultureIgnoreCase));

        var valueStr = CommonHelper.To<string>(value);

        if (prop != null)
        {
            if (string.IsNullOrWhiteSpace(valueStr))
                await DeleteAttributeAsync(prop);
            else
            {
                prop.Value = valueStr;
                await UpdateAttributeAsync(prop);
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(valueStr))
                return;

            prop = new GenericAttribute
            {
                EntityId = entity.Id,
                Key = key,
                KeyGroup = keyGroup,
                Value = valueStr
            };

            await InsertAttributeAsync(prop);
        }
    }

    public async Task InsertAttributeAsync(GenericAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        attribute.CreatedOrUpdatedDate = DateTime.UtcNow;

        await _genericAttributeRepository.InsertAsync(attribute);
    }

    public async Task UpdateAttributeAsync(GenericAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        attribute.CreatedOrUpdatedDate = DateTime.UtcNow;

        await _genericAttributeRepository.UpdateAsync(attribute);
    }

    public async Task DeleteAttributeAsync(GenericAttribute attribute)
    {
        await _genericAttributeRepository.DeleteAsync(attribute);
    }

    public async Task DeleteAttributesAsync(IList<GenericAttribute> attributes)
    {
        await _genericAttributeRepository.DeleteAsync(attributes);
    }

    #endregion
}