using System.Linq.Dynamic.Core;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace Taskist.Service.Masters;

public class StatusService : IStatusService
{
    #region Fields

    protected readonly IRepository<Status> _statusRepository;

    #endregion

    #region Ctor
    public StatusService(IRepository<Status> departmentRepository)
    {
        _statusRepository = departmentRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<Status>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _statusRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<Status>> GetAllAsync(bool showDeleted = false)
    {
        return await _statusRepository.GetAllAsync(includeDeleted: showDeleted);
    }

    public async Task<IList<Status>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _statusRepository.GetAllAsync(q => q.Where(x => x.Active), showDeleted);
    }

    public async Task<Status> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _statusRepository.GetByIdAsync(id);
    }

    public async Task<Status> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _statusRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(Status entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _statusRepository.InsertAsync(entity);
    }

    public async Task InsertAsync(List<Status> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _statusRepository.InsertAsync(entities);
    }

    public async Task UpdateAsync(Status entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _statusRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Status entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _statusRepository.DeleteAsync(entity);
    }

    #endregion
}
