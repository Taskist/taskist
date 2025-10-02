using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public partial interface IGenericAttributeService
{
    Task<IList<GenericAttribute>> GetAttributesForEntityAsync(int entityId, string keyGroup);

    Task<TPropType> GetAttributeAsync<TPropType>(BaseEntity entity, string key, TPropType defaultValue = default);

    Task<TPropType> GetAttributeAsync<TEntity, TPropType>(int entityId, string key, TPropType defaultValue = default) where TEntity : BaseEntity;

    Task SaveAttributeAsync<TPropType>(BaseEntity entity, string key, TPropType value);

    Task InsertAttributeAsync(GenericAttribute attribute);

    Task UpdateAttributeAsync(GenericAttribute attribute);

    Task DeleteAttributeAsync(GenericAttribute attribute);

    Task DeleteAttributesAsync(IList<GenericAttribute> attributes);
}