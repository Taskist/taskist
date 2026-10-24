using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Git;

namespace Taskist.Service.WorkItems;

public interface IMyCodeService
{
    Task<MyCodeDto> GetAsync(User user, bool refresh = false);

    Task ClearCacheAsync(int userId);
}
