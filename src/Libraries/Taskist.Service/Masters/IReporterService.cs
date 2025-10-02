using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface IReporterService
{
    Task<IPagedList<Reporter>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Reporter>> GetAllAsync(bool showDeleted = false);

    Task<IList<Reporter>> GetAllActiveAsync(bool showDeleted = false);

    Task<Reporter> GetByIdAsync(int id);

    Task<Reporter> GetByNameAsync(string name);

    Task InsertAsync(Reporter entity);

    Task UpdateAsync(Reporter entity);

    Task DeleteAsync(Reporter entity);
}
