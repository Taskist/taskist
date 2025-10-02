using System.Linq.Dynamic.Core;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;

namespace Taskist.Service.Masters;

public class ReporterService : IReporterService
{
    #region Fields

    protected readonly IRepository<Reporter> _reporterRepository;

    #endregion

    #region Ctor
    public ReporterService(IRepository<Reporter> reporterRepository)
    {
        _reporterRepository = reporterRepository;
    }
    #endregion

    #region Methods

    public async Task<IPagedList<Reporter>> GetPagedListAsync(string search = "", int pageIndex = 0,
    int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _reporterRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }
    public async Task<IList<Reporter>> GetAllAsync(bool showDeleted = false)
    {
        return await _reporterRepository.GetAllAsync(includeDeleted: showDeleted);
    }

    public async Task<IList<Reporter>> GetAllActiveAsync(bool showDeleted = false)
    {
        return await _reporterRepository.GetAllAsync(q => q.Where(x => x.Active), showDeleted);
    }

    public async Task<Reporter> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _reporterRepository.GetByIdAsync(id);
    }

    public async Task<Reporter> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _reporterRepository.Table
                    orderby c.Id
                    where !c.Deleted && c.Name == name
                    select c;
        return await query.FirstOrDefaultAsync();
    }

    public async Task InsertAsync(Reporter entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _reporterRepository.InsertAsync(entity);
    }

    public async Task UpdateAsync(Reporter entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _reporterRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(Reporter entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _reporterRepository.DeleteAsync(entity);
    }

    #endregion
}
