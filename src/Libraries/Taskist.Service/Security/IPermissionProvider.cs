using Taskist.Core.Domain.Users;

namespace Taskist.Service.Security;

public interface IPermissionProvider
{
    IEnumerable<UserRolePermission> GetPermissions();
}
