using Taskist.Core.Domain.Users;

namespace Taskist.Service.Security;

public interface IPermissionService
{
    Task<UserRolePermission> GetByIdAsync(int permissionId);

    Task<UserRolePermission> GetBySystemNameAsync(string systemName);

    Task<IList<UserRolePermission>> GetByRoleAsync(string roleName);

    Task<IList<UserRolePermission>> GetByRoleIdAsync(int userRoleId);

    Task<IList<UserRolePermission>> GetAllUserRolePermissionsAsync();

    Task InsertUserRolePermissionAsync(UserRolePermission entity);

    Task InsertUserRolePermissionAsync(IList<UserRolePermission> entities);

    Task UpdateUserRolePermissionAsync(UserRolePermission entity);

    Task<bool> AuthorizeAsync(UserRolePermission entity);

    Task<bool> AuthorizeAsync(UserRolePermission permission, User user);

    Task<bool> AuthorizeAsync(string userRoleSystemName);

    Task<bool> AuthorizeAsync(string userRoleSystemName, User user);

    Task DeleteUserRolePermissionAsync(UserRolePermission permission);

    Task DeleteAllPermissionMapAsync(int userRoleId);
}