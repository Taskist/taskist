using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;

namespace Taskist.Service.Masters;

public class SubModuleService : ISubModuleService
{
    #region Fields

    protected readonly IRepository<SubModule> _subModuleRepository;

    #endregion

    #region Ctor
    public SubModuleService(IRepository<SubModule> subModuleRepository)
    {
        _subModuleRepository = subModuleRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<SubModule>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _subModuleRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<SubModule>> GetAllAsync(bool showDeleted = false)
    {
        return await _subModuleRepository.GetAllAsync(includeDeleted: showDeleted);
    }

    public async Task<IList<SubModule>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _subModuleRepository.GetAllAsync(q => q.Where(x => x.Active), showDeleted);
    }

    public async Task<IList<SubModule>> GetAllActiveByModuleAsync(int moduleId)
    {
        return await _subModuleRepository.GetAllAsync(q => q.Where(x => x.Active && x.ModuleId == moduleId));
    }

    public async Task<SubModule> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _subModuleRepository.GetByIdAsync(id);
    }

    public async Task<SubModule> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _subModuleRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(SubModule entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _subModuleRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(IList<SubModule> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _subModuleRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(SubModule entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _subModuleRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(SubModule entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _subModuleRepository.DeleteAsync(entity);
    }

    #endregion
}
