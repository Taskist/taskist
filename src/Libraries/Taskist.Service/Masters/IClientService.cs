using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Masters;

public interface IClientService
{
    Task<IPagedList<Client>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
       string sortColumn = "", string sortDirection = "");

    Task<IList<Client>> GetAllAsync(bool showDeleted = false);

    Task<IList<Client>> GetAllActiveAsync(bool showDeleted = false);

    Task<Client> GetByIdAsync(int id);

    Task<Client> GetByNameAsync(string name);

    Task InsertAsync(Client entity);

    Task InsertAsync(IList<Client> entities);

    Task UpdateAsync(Client entity);

    Task DeleteAsync(Client entity);
}
