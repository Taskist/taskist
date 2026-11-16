using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;

namespace Taskist.Service.Users;

public interface IUserService
{
    #region Users

    Task<IPagedList<User>> GetPagedListAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "", int[] statuses = null, int[] roleIds = null, bool? locked = null);

    Task<IPagedList<UserRole>> GetPagedListUserRoleAsync(string search = "", int pageIndex = 0, int pageSize = int.MaxValue,
        string sortColumn = "", string sortDirection = "");

    Task<IList<User>> GetAllAsync();

    Task<IList<User>> GetAllActiveAsync();

    Task<IList<User>> GetAllActiveByProjectAsync(int projectId);

    Task<IList<User>> GetAllActiveByProjectAsync(int projectId, string roleName);

    Task<IList<User>> GetAllActiveByRoleAsync(string roleName);

    Task<IList<User>> GetAllInactiveAsync();

    Task<IList<User>> GetAllInactiveByRoleAsync(string roleName);

    Task<IList<User>> GetActiveByRolesAsync(IEnumerable<string> roleNames);

    Task<User> GetByIdAsync(int id);

    Task<User> GetByCodeAsync(Guid code);

    Task<User> GetByEmailAsync(string email);

    Task<User> GetByNameAsync(string name);

    Task<User> GetByGitUserAsync(string gitUsername);

    Task<User> GetByMobileNumberAsync(string mobileNo);

    Task<string> GenerateCode();

    Task InsertAsync(User entity);

    Task UpdateAsync(User entity);

    Task DeleteAsync(User entity);

    Task<IList<User>> GetUserByPermissionAsync(string permissionSystemName);

    Task<IList<int>> GetProjectLeadIdsForUserAsync(int userId);

    Task<Dictionary<int, List<string>>> GetActiveUserProjectNamesAsync();

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

    Task<IList<UserRoleMap>> GetAllUserRoleMapsAsync();

    Task InsertUserRoleAsync(UserRole userRole);

    Task InsertUserRoleAsync(List<UserRole> userRoles);

    Task<bool> IsInUserRoleAsync(User user, string userRoleSystemName, bool onlyActiveRoles = true);

    Task<bool> IsAdminAsync(User user, bool onlyActiveRoles = true);

    Task<bool> IsRegisteredAsync(User user, bool onlyActiveRoles = true);

    #endregion

    #region User passwords

    Task<IList<UserPassword>> GetUserPasswordsAsync(int? userId = null, int? passwordsToReturn = null);

    Task<UserPassword> GetCurrentPasswordAsync(int userId);

    Task<bool> ValidateCurrentPasswordAsync(int userId, string password);

    Task InsertUserPasswordAsync(UserPassword userPassword);

    Task UpdateUserPasswordAsync(UserPassword userPassword);

    #endregion

    #region Authentication/Registration

    Task<LoginResult> ValidateAsync(string userName, string password);

    Task<bool> UnlockAsync(int userId);

    Task ResetPasswordAsync(int userId, string newPassword);

    Task<RegistrationResult> RegisterAsync(User user,
        Address address,
        List<int> roleIds,
        User loggedUser,
        bool emailWelcomeKit);

    Task<User> ValidateTokenAsync(string token);

    #endregion

    #region Projects

    Task<IList<Project>> GetAllAccessibleProjects(int userId, bool fetchOnlyCanViewOthersTask = false);

    Task<UserProjectMap> GetProjectMapping(int userId, int projectId);

    #endregion
}