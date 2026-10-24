using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Data.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Common;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;

namespace Taskist.Service.Users;

public class UserService : IUserService
{
    #region Fields

    protected readonly UserSettings _userSettings;
    protected readonly IRepository<User> _userRepository;
    protected readonly IRepository<UserPassword> _passwordRepository;
    protected readonly IRepository<UserRole> _userRoleRepository;
    protected readonly IRepository<UserRolePermission> _userRolePermissionRepository;
    protected readonly IRepository<UserRoleMap> _userRoleMapRepository;
    protected readonly IRepository<UserRolePermissionMap> _permissionMapRepository;
    protected readonly IRepository<UserProjectMap> _userProjectMapRepository;
    protected readonly IAddressService _addressService;
    protected readonly IMessageService _messageService;
    protected readonly IEncryptionService _encryptionService;
    protected readonly ISettingService _settingsService;
    protected readonly ICacheManager _cacheManager;

    #endregion

    #region Ctor

    public UserService(UserSettings userSettings,
            IRepository<User> userRepository,
            IRepository<UserPassword> passwordRepository,
            IRepository<UserRole> userRoleRepository,
            IRepository<UserRolePermission> permissionRepository,
            IRepository<UserRoleMap> userRoleMapRepository,
            IRepository<UserRolePermissionMap> permissionMapRepository,
            IRepository<UserProjectMap> userProjectMapRepository,
            IEncryptionService encryptionService,
            ISettingService settingsService,
            IAddressService addressService,
            IMessageService messageService,
            ICacheManager cacheManager)
    {
        _userSettings = userSettings;
        _userRepository = userRepository;
        _passwordRepository = passwordRepository;
        _userRoleRepository = userRoleRepository;
        _userRolePermissionRepository = permissionRepository;
        _userRoleMapRepository = userRoleMapRepository;
        _permissionMapRepository = permissionMapRepository;
        _userProjectMapRepository = userProjectMapRepository;
        _encryptionService = encryptionService;
        _settingsService = settingsService;
        _addressService = addressService;
        _messageService = messageService;
        _cacheManager = cacheManager;
    }

    #endregion

    #region Users

    public async Task<IPagedList<User>> GetPagedListAsync(string search = "", int pageIndex = 0,
            int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "", int[] statuses = null,
            int[] roleIds = null, bool? locked = null)
    {
        return await _userRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.SystemAccount);
            query = query.ApplySafeSort(sortColumn, sortDirection);

            if (!string.IsNullOrWhiteSpace(search))
                query =
                    query.Where(
                        c =>
                            c.FirstName.Contains(search) ||
                            c.LastName.Contains(search) ||
                            c.Email.Contains(search) ||
                            c.MobileNumber.Contains(search));

            if (statuses is { Length: > 0 })
                query = query.Where(c => statuses.Contains(c.Status));

            if (roleIds is { Length: > 0 })
                query = query.Where(c => c.UserUserRoleMaps.Any(m => roleIds.Contains(m.UserRoleId)));

            if (locked.HasValue)
                query = query.Where(c => c.Locked == locked.Value);

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IPagedList<UserRole>> GetPagedListUserRoleAsync(string search = "", int pageIndex = 0,
            int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _userRoleRepository.GetAllPagedAsync(query =>
        {
            query = query.ApplySafeSort(sortColumn, sortDirection);

            if (!string.IsNullOrWhiteSpace(search))
                query =
                    query.Where(
                        c => c.Name.Contains(search) ||
                             c.Description.Contains(search) ||
                             c.SystemName.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IList<UserRolePermissionMap>> GetAllUserRolePermissionMapsAsync()
    {
        return await _permissionMapRepository.GetAllAsync(includeDeleted: false);
    }

    public async Task<IList<UserRoleMap>> GetAllUserRoleMapsAsync()
    {
        return await _userRoleMapRepository.GetAllAsync(includeDeleted: false);
    }

    public async Task<IList<User>> GetAllAsync()
    {
        return SortByName(await _userRepository.GetAllAsync());
    }

    public async Task<IList<User>> GetAllActiveAsync()
    {
        return SortByName(await _userRepository.GetAllAsync(query => query.Where(x =>
            x.Status == (int)UserStatus.Active &&
            !x.SystemAccount), false));
    }

    public async Task<IList<User>> GetAllActiveByRoleAsync(string roleName)
    {
        return SortByName(await _userRepository.GetAllAsync(query => query.Where(x =>
            x.Status == (int)UserStatus.Active &&
            !x.SystemAccount &&
            x.UserUserRoleMaps.Any(y => y.UserRole.SystemName == roleName)), false));
    }

    public async Task<IList<User>> GetAllInactiveAsync()
    {
        return SortByName(await _userRepository.GetAllAsync(query => query.Where(x =>
            x.Status != (int)UserStatus.Active &&
            !x.SystemAccount), false));
    }

    public async Task<IList<User>> GetAllInactiveByRoleAsync(string roleName)
    {
        return SortByName(await _userRepository.GetAllAsync(query => query.Where(x =>
            x.Status != (int)UserStatus.Active &&
            !x.SystemAccount &&
            x.UserUserRoleMaps.Any(y => y.UserRole.SystemName == roleName)), false));
    }

    public async Task<IList<User>> GetActiveByRolesAsync(IEnumerable<string> roleNames)
    {
        var roles = (roleNames ?? []).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct().ToList();
        if (roles.Count == 0)
            return [];

        return SortByName(await _userRepository.GetAllAsync(query => query.Where(x =>
            x.Status == (int)UserStatus.Active &&
            !x.SystemAccount &&
            x.UserUserRoleMaps.Any(y => roles.Contains(y.UserRole.SystemName))), false));
    }

    public async Task<IList<User>> GetAllActiveByProjectAsync(int projectId)
    {
        var query = _userProjectMapRepository.Table.Where(x =>
        (x.User.Status == (int)UserStatus.Active && !x.User.SystemAccount));

        if (projectId > 0)
            query = query.Where(x => x.ProjectId == projectId);

        return SortByName(await query.Select(s => s.User).Distinct().ToListAsync());
    }

    public async Task<IList<User>> GetAllActiveByProjectAsync(int projectId, string roleName)
    {
        var query = _userProjectMapRepository.Table.Where(x =>
        (x.User.Status == (int)UserStatus.Active && !x.User.SystemAccount) &&
        x.User.UserUserRoleMaps.Any(y => y.UserRole.SystemName == roleName));

        if (projectId > 0)
            query = query.Where(x => x.ProjectId == projectId);

        return SortByName(await query.Select(s => s.User).Distinct().ToListAsync());
    }

    public async Task<IList<int>> GetProjectLeadIdsForUserAsync(int userId)
    {
        var projectIds = _userProjectMapRepository.Table
            .Where(x => x.UserId == userId)
            .Select(x => x.ProjectId);

        return await _userProjectMapRepository.Table
            .Where(x => projectIds.Contains(x.ProjectId)
                && x.UserId != userId
                && x.User.Status == (int)UserStatus.Active && !x.User.SystemAccount
                && x.User.UserUserRoleMaps.Any(y =>
                    y.UserRole.SystemName == UserConstant.ProjectManagerRoleName
                    || y.UserRole.SystemName == UserConstant.TeamLeadRoleName))
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<Dictionary<int, List<string>>> GetActiveUserProjectNamesAsync()
    {
        var rows = await _userProjectMapRepository.Table
            .Where(x => x.Project.Active && !x.Project.Deleted)
            .Select(x => new { x.UserId, x.Project.Name })
            .ToListAsync();

        return rows
            .GroupBy(r => r.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Name).Distinct().OrderBy(n => n).ToList());
    }

    public async Task<IList<User>> GetAllAssigneeAsync()
    {
        return SortByName(await _userRepository.GetAllAsync(query => query.Where(x => x.Status == (int)UserStatus.Active && !x.SystemAccount), false));
    }

    public async Task<User> GetByIdAsync(int id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User> GetByCodeAsync(Guid code)
    {
        if (code == Guid.Empty)
            return null;

        var query = from c in _userRepository.Table
                    where c.Code == code
                    select c;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var query = from c in _userRepository.Table
                    where c.Email == email
                    select c;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<User> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = from c in _userRepository.Table
                    where c.Name == name
                    select c;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<User> GetByGitUserAsync(string gitUsername)
    {
        if (string.IsNullOrWhiteSpace(gitUsername))
            return null;

        var query = from c in _userRepository.Table
                    where c.GitUserName == gitUsername
                    select c;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<User> GetByMobileNumberAsync(string mobileNo)
    {
        var query = from c in _userRepository.Table
                    where c.MobileNumber == mobileNo
                    select c;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<string> GenerateCode()
    {
        var prefix = _userSettings.Prefix;
        var sequence = _userSettings.Sequence;

        if (string.IsNullOrEmpty(prefix))
            return null;

        return $"{prefix}{sequence + 1}";
    }

    public async Task InsertAsync(User entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _userRepository.InsertAsync(entity);
    }

    public async Task UpdateAsync(User entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _userRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(User entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.SystemAccount)
            throw new Exception($"System entity account ({entity.Email}) could not be deleted");

        await _userRepository.DeleteAsync(entity);
    }

    public async Task<IList<User>> GetUserByPermissionAsync(string permissionSystemName)
    {
        if (string.IsNullOrWhiteSpace(permissionSystemName))
            return new List<User>();

        var query = from e in _userRepository.Table
                    join rm in _userRoleMapRepository.Table on e.Id equals rm.UserId
                    join pm in _permissionMapRepository.Table on rm.UserRoleId equals pm.UserRoleId
                    join p in _userRolePermissionRepository.Table on pm.PermissionId equals p.Id
                    where !e.Deleted && p.SystemName == permissionSystemName
                    select e;

        return SortByName(await query.Distinct().ToListAsync());
    }

    #endregion

    #region User roles

    public async Task AddUserRoleMappingAsync(UserRoleMap roleMapping)
    {
        await _userRoleMapRepository.InsertAsync(roleMapping);
    }

    public async Task UpdateUserRoleAsync(UserRole userRole)
    {
        await _userRoleRepository.UpdateAsync(userRole);
        await _cacheManager.RemoveAsync(ServiceConstant.UserRolesAllCacheKey);
    }

    public async Task RemoveUserRoleMappingAsync(User user, UserRole role)
    {
        if (user is null)
            throw new ArgumentNullException(nameof(user));

        if (role is null)
            throw new ArgumentNullException(nameof(role));

        var mapping = await _userRoleMapRepository.Table
                    .SingleOrDefaultAsync(x => x.UserId == user.Id && x.UserRoleId == role.Id);

        if (mapping != null)
            await _userRoleMapRepository.DeleteAsync(mapping);
    }

    public async Task DeleteUserRoleAsync(UserRole userRole)
    {
        if (userRole == null)
            throw new ArgumentNullException(nameof(userRole));

        if (userRole.SystemDefined)
            throw new Exception("System role could not be deleted");

        await _userRoleRepository.DeleteAsync(userRole);
    }

    public async Task<UserRole> GetUserRoleByIdAsync(int userRoleId)
    {
        var query = _userRoleRepository.Table.Where(x => x.Id == userRoleId);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<UserRole> GetUserRoleBySystemNameAsync(string systemName)
    {
        if (string.IsNullOrWhiteSpace(systemName))
            return null;

        var query = _userRoleRepository.Table.Where(x => x.SystemName == systemName);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<UserRole> GetUserRoleByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var query = _userRoleRepository.Table.Where(x => x.Name == name);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<int[]> GetUserRoleIdsAsync(User user, bool showHidden = false)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var query = from cr in _userRoleRepository.Table
                    join crm in _userRoleMapRepository.Table on cr.Id equals crm.UserRoleId
                    where crm.UserId == user.Id &&
                    (showHidden || cr.Active)
                    select cr.Id;

        return await _cacheManager.GetAsync(ServiceConstant.UserRoleIdsCacheKey, () => query.ToArrayAsync());
    }

    public async Task<IList<UserRole>> GetUserRolesAsync(User user, bool showHidden = false)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var query = from cr in _userRoleRepository.Table
                    join crm in _userRoleMapRepository.Table on cr.Id equals crm.UserRoleId
                    where crm.UserId == user.Id &&
                    (showHidden || cr.Active)
                    select cr;

        return await query.ToListAsync();
    }

    public async Task<IList<UserRole>> GetAllUserRolesAsync(bool showHidden = false)
    {
        var query = from cr in _userRoleRepository.Table
                    orderby cr.Name
                    where showHidden || cr.Active
                    select cr;

        var userRoles = await _cacheManager.GetAsync(ServiceConstant.UserRolesAllCacheKey, () => query.ToListAsync());

        return userRoles;
    }

    public async Task InsertUserRoleAsync(UserRole userRole)
    {
        await _userRoleRepository.InsertAsync(userRole);

        if (userRole.Id > 0)
            await _cacheManager.RemoveAsync(ServiceConstant.UserRolesAllCacheKey);
    }

    public async Task InsertUserRoleAsync(List<UserRole> userRoles)
    {
        ArgumentNullException.ThrowIfNull(userRoles);

        await _userRoleRepository.InsertAsync(userRoles);
    }

    public async Task<bool> IsInUserRoleAsync(User user,
            string userRoleSystemName, bool onlyActiveUserRoles = true)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(userRoleSystemName))
            throw new ArgumentNullException(nameof(userRoleSystemName));

        var userRoles = await GetUserRolesAsync(user, !onlyActiveUserRoles);

        return userRoles?.Any(cr => cr.SystemName == userRoleSystemName) ?? false;
    }

    public async Task<bool> IsAdminAsync(User user, bool onlyActiveUserRoles = true)
    {
        return await IsInUserRoleAsync(user, UserConstant.AdministratorsRoleName, onlyActiveUserRoles);
    }

    public async Task<bool> IsRegisteredAsync(User user, bool onlyActiveUserRoles = true)
    {
        return await IsInUserRoleAsync(user, UserConstant.RegisteredRoleName, onlyActiveUserRoles);
    }

    #endregion

    #region User passwords

    public async Task<IList<UserPassword>> GetUserPasswordsAsync(int? userId = null, int? passwordsToReturn = null)
    {
        var query = _passwordRepository.Table;

        if (userId.HasValue)
            query = query.Where(password => password.UserId == userId.Value);

        if (passwordsToReturn.HasValue)
            query = query.OrderByDescending(password => password.CreatedOn).Take(passwordsToReturn.Value);

        return await query.ToListAsync();
    }

    public async Task<UserPassword> GetCurrentPasswordAsync(int userId)
    {
        if (userId == 0)
            return null;

        var currPassword = await GetUserPasswordsAsync(userId, passwordsToReturn: 1);
        return currPassword.FirstOrDefault();
    }

    public async Task InsertUserPasswordAsync(UserPassword userPassword)
    {
        await _passwordRepository.InsertAsync(userPassword);
    }

    public async Task UpdateUserPasswordAsync(UserPassword userPassword)
    {
        await _passwordRepository.UpdateAsync(userPassword);
    }

    #endregion

    #region Authentication/Registration

    public async Task<LoginResult> ValidateAsync(string email, string password)
    {
        var entity = await GetByEmailAsync(email);
        if (entity == null)
            return LoginResult.NotExist;
        if (entity.Deleted)
            return LoginResult.Deleted;
        if (entity.Status != (int)UserStatus.Active)
            return LoginResult.NotActive;
        if (entity.Locked)
            return LoginResult.LockedOut;

        var activePassword = await GetCurrentPasswordAsync(entity.Id);

        if (activePassword == null)
            return LoginResult.LockedOut;

        var check = _encryptionService.VerifyPassword(password, activePassword.Password);
        if (check == PasswordCheckResult.Failed)
        {
            var security = await _settingsService.LoadSettingAsync<SecuritySettings>();
            if (security.MaxFailedLoginAttempts > 0)
            {
                entity.FailedLoginAttempts++;
                if (entity.FailedLoginAttempts >= security.MaxFailedLoginAttempts)
                    entity.Locked = true;
                await _userRepository.UpdateAsync(entity);
            }
            return LoginResult.WrongPassword;
        }

        if (check == PasswordCheckResult.SuccessRehashNeeded)
        {
            activePassword.Password = _encryptionService.HashPassword(password);
            activePassword.PasswordSalt = string.Empty;
            await UpdateUserPasswordAsync(activePassword);
        }

        if (!await IsRegisteredAsync(entity))
            return LoginResult.NotRegistered;

        if (entity.FailedLoginAttempts != 0)
        {
            entity.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(entity);
        }

        return LoginResult.Successful;
    }

    public async Task<bool> UnlockAsync(int userId)
    {
        var entity = await GetByIdAsync(userId);
        if (entity == null)
            return false;

        entity.Locked = false;
        entity.FailedLoginAttempts = 0;
        await _userRepository.UpdateAsync(entity);

        return true;
    }

    public async Task<bool> ValidateCurrentPasswordAsync(int userId, string password)
    {
        var activePassword = await GetCurrentPasswordAsync(userId);

        if (activePassword == null)
            return false;

        return _encryptionService.VerifyPassword(password, activePassword.Password) != PasswordCheckResult.Failed;
    }

    public async Task ResetPasswordAsync(int userId, string newPassword)
    {
        var entity = await GetByIdAsync(userId);

        var newEntity = new UserPassword
        {
            User = entity,
            CreatedOn = DateTime.UtcNow
        };

        var saltKey = string.Empty;
        var encryptedNewPassword = _encryptionService.HashPassword(newPassword);

        var currentPassword = await GetCurrentPasswordAsync(entity.Id);
        if (currentPassword != null)
        {
            currentPassword.PasswordSalt = saltKey;
            currentPassword.Password = encryptedNewPassword;
            await UpdateUserPasswordAsync(currentPassword);
        }
        else
        {
            newEntity.PasswordSalt = saltKey;
            newEntity.Password = encryptedNewPassword;
            await InsertUserPasswordAsync(newEntity);
        }
    }

    public async Task<RegistrationResult> RegisterAsync(User user,
            Address address,
            List<int> roleIds,
            User loggedUser,
            bool emailWelcomeKit)
    {
        var newAddressEntity = await _addressService.InsertAsync(address);
        if (newAddressEntity.Id > 0)
        {
            user.Code = Guid.NewGuid();
            user.Status = (int)UserStatus.Active;
            user.LanguageId = loggedUser.LanguageId;
            user.AddressId = newAddressEntity.Id;
            user.SystemAccount = false;
            user.Deleted = false;

            await InsertAsync(user);
            var newUserEntity = await GetByEmailAsync(user.Email);
            if (newUserEntity != null && newUserEntity.Id > 0)
            {
                var registeredRole = await GetUserRoleBySystemNameAsync(UserConstant.RegisteredRoleName);

                if (registeredRole == null)
                    throw new Exception("Registered role not found");

                if (!roleIds.Any(x => x == registeredRole.Id))
                    newUserEntity.AddToRole(new UserRoleMap { UserRole = registeredRole });

                foreach (var roleId in roleIds)
                {
                    newUserEntity.AddToRole(new UserRoleMap { UserId = newUserEntity.Id, UserRoleId = roleId });
                }

                await UpdateAsync(newUserEntity);

                if (emailWelcomeKit)
                {
                    await _messageService.EmailActivationKitAsync(newUserEntity);
                }

                return RegistrationResult.Successful;
            }
        }

        return RegistrationResult.Failed;
    }

    public async Task<User> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var decryptedToken = _encryptionService.DecryptText(token);
            var tokenArr = decryptedToken.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokenArr.Length != 2)
                return null;

            if (Guid.TryParse(tokenArr[0].ToString(), out Guid code))
            {
                var user = await GetByCodeAsync(code);
                if (user == null || user.Id <= 0 || user.Deleted || user.Status != (int)UserStatus.Active)
                    return null;

                var security = await _settingsService.LoadSettingAsync<SecuritySettings>();
                var expiryMinutes = security.TokenExpiryMinutes > 0 ? security.TokenExpiryMinutes : 60;

                var isValidToken = _encryptionService.ValidateToken(tokenArr[1].ToString(), expiryMinutes);
                if (!isValidToken)
                    return null;

                return user;
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion

    #region Projects

    public async Task<IList<Project>> GetAllAccessibleProjects(int userId, bool fetchOnlyCanViewOthersTask = false)
    {
        var query = _userProjectMapRepository.Table
            .Where(x => x.Project.Active && !x.Project.Deleted && x.UserId == userId)
            .Select(s => s);

        if (fetchOnlyCanViewOthersTask)
            query = query.Where(x => x.CanViewOthersTask);

        var key = string.Format(ServiceConstant.AccessibleProjectCacheKey, userId, fetchOnlyCanViewOthersTask ? "viewall" : "all");

        return await _cacheManager.GetAsync(key, () => query.Select(s => s.Project).ToListAsync());
    }

    public async Task<UserProjectMap> GetProjectMapping(int userId, int projectId)
    {
        var query = _userProjectMapRepository.Table
            .Where(x => x.Project.Active && !x.Project.Deleted && x.UserId == userId && x.ProjectId == projectId);

        return await query.FirstOrDefaultAsync();
    }

    #endregion

    #region Utilities

    private static IList<User> SortByName(IEnumerable<User> users)
    {
        return users.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToList();
    }

    #endregion
}