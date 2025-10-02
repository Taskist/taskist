using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface ISeverityService
{
    Task<IPagedList<Severity>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Severity>> GetAllAsync(bool showDeleted = false);

    Task<IList<Severity>> GetAllActiveAsync(bool showDeleted = false);

    Task<Severity> GetByIdAsync(int id);

    Task<Severity> GetByNameAsync(string name);

    Task InsertAsync(Severity entity);

    Task InsertAsync(IList<Severity> entities);

    Task UpdateAsync(Severity entity);

    Task DeleteAsync(Severity entity);
}
