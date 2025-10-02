using System.Linq.Dynamic.Core;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace Taskist.Service.Masters;

public class TaskTypeService : ITaskTypeService
{
    #region Fields

    protected readonly IRepository<TaskType> _taskTypeRepository;

    #endregion

    #region Ctor
    public TaskTypeService(IRepository<TaskType> departmentRepository)
    {
        _taskTypeRepository = departmentRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<TaskType>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _taskTypeRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<TaskType>> GetAllAsync(bool showDeleted = false)
    {
        return await _taskTypeRepository.GetAllAsync(includeDeleted: showDeleted);
    }

    public async Task<IList<TaskType>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _taskTypeRepository.GetAllAsync(q => q.Where(x => x.Active), showDeleted);
    }

    public async Task<TaskType> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _taskTypeRepository.GetByIdAsync(id);
    }

    public async Task<TaskType> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _taskTypeRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(TaskType entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _taskTypeRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(List<TaskType> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _taskTypeRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(TaskType entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _taskTypeRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(TaskType entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _taskTypeRepository.DeleteAsync(entity);
    }

    #endregion
}
