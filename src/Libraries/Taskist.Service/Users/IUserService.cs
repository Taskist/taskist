using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Users;

public interface IUserService
{
    #region Users

    Task<IPagedList<User>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "");

    Task<IPagedList<UserRole>> GetPagedListUserRoleAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "");

    Task<IList<User>> GetAllAsync();

    Task<IList<User>> GetAllActiveAsync();

    Task<IList<User>> GetAllActiveByProjectAsync(int projectId);

    Task<User> GetByIdAsync(int id);

    Task<User> GetByCodeAsync(Guid code);

    Task<User> GetByEmailAsync(string email);

    Task InsertAsync(User entity);

    Task UpdateAsync(User entity);

    Task DeleteAsync(User entity);

    #endregion

    #region User roles

    Task AddUserRoleMappingAsync(UserRoleMap roleMapping);

    Task UpdateUserRoleAsync(UserRole userRole);

    Task RemoveUserRoleMappingAsync(User user, UserRole role);

    Task DeleteUserRoleAsync(UserRole userRole);

    Task<UserRole> GetUserRoleByIdAsync(int userRoleId);

    Task<UserRole> GetUserRoleBySystemNameAsync(string systemName);

    Task<UserRole> GetUserRoleByNameAsync(string systemName);

    Task<int[]> GetUserRoleIdsAsync(User user, bool showHidden = false);

    Task<IList<UserRole>> GetUserRolesAsync(User user, bool showHidden = false);

    Task<IList<UserRole>> GetAllUserRolesAsync(bool showHidden = false);

    Task<IList<UserRolePermissionMap>> GetAllUserRolePermissionMapsAsync();

    Task InsertUserRoleAsync(UserRole userRole);

    Task InsertUserRoleAsync(List<UserRole> userRoles);

    Task<bool> IsInUserRoleAsync(User user, string userRoleSystemName, bool onlyActiveRoles = true);

    Task<bool> IsAdminAsync(User user, bool onlyActiveRoles = true);

    Task<bool> IsRegisteredAsync(User user, bool onlyActiveRoles = true);

    #endregion

    #region User passwords

    Task<IList<UserPassword>> GetUserPasswordsAsync(int? userId = null, int? passwordsToReturn = null);

    Task<UserPassword> GetCurrentPasswordAsync(int userId);

    Task InsertUserPasswordAsync(UserPassword userPassword);

    Task UpdateUserPasswordAsync(UserPassword userPassword);

    #endregion

    #region Authentication/Registration

    Task<LoginResultEnum> ValidateAsync(string userName, string password);

    Task ResetPasswordAsync(int userId, string newPassword);

    Task<RegistrationResultEnum> RegisterAsync(User user,
        List<int> roleIds,
        User loggedUser,
        bool emailWelcomeKit);

    Task<User> ValidateTokenAsync(string token);

    #endregion

    #region Projects

    Task<IList<Project>> GetAllAccessibleProjects(int userId);

    Task<UserProjectMap> GetProjectMapping(int userId, int projectId);

    #endregion
}