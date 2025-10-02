using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface ISubModuleService
{
    Task<IPagedList<SubModule>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<SubModule>> GetAllAsync(bool showDeleted = false);

    Task<IList<SubModule>> GetAllActiveAsync(bool showDeleted = false);

    Task<IList<SubModule>> GetAllActiveByModuleAsync(int moduleId);

    Task<SubModule> GetByIdAsync(int id);

    Task<SubModule> GetByNameAsync(string name);

    Task InsertAsync(SubModule entity);

    Task InsertAsync(IList<SubModule> entities);

    Task UpdateAsync(SubModule entity);

    Task DeleteAsync(SubModule entity);
}
