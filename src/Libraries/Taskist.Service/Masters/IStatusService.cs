using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface IStatusService
{
    Task<IPagedList<Status>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Status>> GetAllAsync(bool showDeleted = false);

    Task<IList<Status>> GetAllActiveAsync(bool showDeleted = false);

    Task<Status> GetByIdAsync(int id);

    Task<Status> GetByNameAsync(string name);

    Task InsertAsync(Status entity);

    Task InsertAsync(List<Status> entities);

    Task UpdateAsync(Status entity);

    Task DeleteAsync(Status entity);
}
