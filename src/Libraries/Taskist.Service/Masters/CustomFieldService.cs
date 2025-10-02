using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;

namespace Taskist.Service.Masters;

public class CustomFieldService : ICustomFieldService
{
    #region Fields

    protected readonly IRepository<CustomField> _customFieldRepository;

    #endregion

    #region Ctor
    public CustomFieldService(IRepository<CustomField> customFieldRepository)
    {
        _customFieldRepository = customFieldRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<CustomField>> GetPagedListAsync(int projectId, string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue)
    {
        return await _customFieldRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => x.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.ResourceKey.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IList<CustomField>> GetAllAsync(int projectId, int placement)
    {
        return await _customFieldRepository.GetAllAsync(q => q.Where(x => x.ProjectId == projectId && x.Placement == placement),
            false);
    }

    public async Task<IList<CustomField>> GetAllMandatoryAsync(int projectId)
    {
        return await _customFieldRepository.GetAllAsync(q => q.Where(x => x.ProjectId == projectId && x.Mandatory), false);
    }

    public async Task<CustomField> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _customFieldRepository.GetByIdAsync(id);
    }

    public async Task<CustomField> GetByLabelAsync(int projectId, string label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return null;

        var query = from c in _customFieldRepository.Table
                    orderby c.Id
                    where c.ProjectId == projectId && c.Label == label
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(CustomField entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _customFieldRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(List<CustomField> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _customFieldRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(CustomField entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _customFieldRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(CustomField entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _customFieldRepository.DeleteAsync(entity);
    }

    #endregion
}
