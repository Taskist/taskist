using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface ITaskTypeService
{
    Task<IPagedList<TaskType>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<TaskType>> GetAllAsync(bool showDeleted = false);

    Task<IList<TaskType>> GetAllActiveAsync(bool showDeleted = false);

    Task<TaskType> GetByIdAsync(int id);

    Task<TaskType> GetByNameAsync(string name);

    Task InsertAsync(TaskType entity);

    Task InsertAsync(List<TaskType> entities);

    Task UpdateAsync(TaskType entity);

    Task DeleteAsync(TaskType entity);
}
