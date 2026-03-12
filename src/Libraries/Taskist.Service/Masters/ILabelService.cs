using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface ILabelService
{
    Task<IPagedList<Label>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Label>> GetAllAsync(bool showDeleted = false);

    Task<IList<Label>> GetAllActiveAsync(bool showDeleted = false);

    Task<Label> GetByIdAsync(int id);

    Task<Label> GetByNameAsync(string name);

    Task InsertAsync(Label entity);

    Task InsertAsync(List<Label> entities);

    Task UpdateAsync(Label entity);

    Task DeleteAsync(Label entity);
}
