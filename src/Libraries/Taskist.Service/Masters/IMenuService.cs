using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Masters;

public interface IMenuService
{
    Task<IList<Menu>> GetAllAsync();

    Task<IList<Menu>> GetAllAsync(User user);

    Task InsertAsync(IList<Menu> entities);
}