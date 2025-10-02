using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Data.Repository;
using Taskist.Service.Common;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;

namespace Taskist.Service.Users;

public class UserService : IUserService
{
    #region Fields

    protected readonly IRepository<User> _userRepository;
    protected readonly IRepository<UserPassword> _passwordRepository;
    protected readonly IRepository<UserRole> _userRoleRepository;
    protected readonly IRepository<UserRolePermission> _userRolePermissionRepository;
    protected readonly IRepository<UserRoleMap> _userRoleMapRepository;
    protected readonly IRepository<UserRolePermissionMap> _permissionMapRepository;
    protected readonly IRepository<UserProjectMap> _userProjectMapRepository;
    protected readonly IMessageService _messageService;
    protected readonly IEncryptionService _encryptionService;
    protected readonly ISettingService _settingsService;
    protected readonly ICacheManager _cacheManager;

    #endregion

    #region Ctor

    public UserService(IRepository<User> userRepository,
        IRepository<UserPassword> passwordRepository,
        IRepository<UserRole> userRoleRepository,
        IRepository<UserRolePermission> permissionRepository,
        IRepository<UserRoleMap> userRoleMapRepository,
        IRepository<UserRolePermissionMap> permissionMapRepository,
        IRepository<UserProjectMap> userProjectMapRepository,
        IEncryptionService encryptionService,
        ISettingService settingsService,
        IMessageService messageService,
        ICacheManager cacheManager)
    {
        _userRepository = userRepository;
        _passwordRepository = passwordRepository;
        _userRoleRepository = userRoleRepository;
        _userRolePermissionRepository = permissionRepository;
        _userRoleMapRepository = userRoleMapRepository;
        _permissionMapRepository = permissionMapRepository;
        _userProjectMapRepository = userProjectMapRepository;
        _encryptionService = encryptionService;
        _settingsService = settingsService;
        _messageService = messageService;
        _cacheManager = cacheManager;
    }

    #endregion

    #region Users

    public async Task<IPagedList<User>> GetPagedListAsync(string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _userRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.SystemAccount);
            query = query.OrderBy($"{sortColumn} {sortDirection}");

            if (!string.IsNullOrWhiteSpace(search))
                query =
                    query.Where(
                        c =>
                            c.FirstName.Contains(search) ||
                            c.LastName.Contains(search) ||
                            c.Email.Contains(search));

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IPagedList<UserRole>> GetPagedListUserRoleAsync(string search = "", int pageIndex = 0,
        int pageSize = int.MaxValue, string sortColumn = "", string sortDirection = "")
    {
        return await _userRoleRepository.GetAllPagedAsync(query =>
        {
            query = query.OrderBy($"{sortColumn} {sortDirection}");

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

    public async Task<IList<User>> GetAllAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<IList<User>> GetAllActiveAsync()
    {
        return await _userRepository.GetAllAsync(query => query.Where(x =>
            x.Status == (int)UserStatusEnum.Active &&
            !x.SystemAccount), false);
    }

    public async Task<IList<User>> GetAllActiveByProjectAsync(int projectId)
    {
        return await _userProjectMapRepository.Table.Where(x =>
        (x.User.Status == (int)UserStatusEnum.Active && !x.User.SystemAccount) && x.ProjectId == projectId)
            .Select(s => s.User).ToListAsync();
    }

    public async Task<IList<User>> GetAllAssigneeAsync()
    {
        return await _userRepository.GetAllAsync(query => query.Where(x => x.Status == (int)UserStatusEnum.Active && !x.SystemAccount), false);
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

        //filter by user
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

    public async Task<LoginResultEnum> ValidateAsync(string email, string password)
    {
        var entity = await GetByEmailAsync(email);
        if (entity == null)
            return LoginResultEnum.NotExist;
        if (entity.Deleted)
            return LoginResultEnum.Deleted;
        if (entity.Status != (int)UserStatusEnum.Active)
            return LoginResultEnum.NotActive;

        var activePassword = await GetCurrentPasswordAsync(entity.Id);

        if (activePassword == null)
            return LoginResultEnum.LockedOut;

        var enteredPassword = _encryptionService.CreatePasswordHash(password, activePassword.PasswordSalt);
        if (!activePassword.Password.Equals(enteredPassword))
            return LoginResultEnum.WrongPassword;

        if (!await IsRegisteredAsync(entity))
            return LoginResultEnum.NotRegistered;

        return LoginResultEnum.Successful;
    }

    public async Task ResetPasswordAsync(int userId, string newPassword)
    {
        var entity = await GetByIdAsync(userId);

        var newEntity = new UserPassword
        {
            User = entity,
            CreatedOn = DateTime.UtcNow
        };

        var saltKey = _encryptionService.CreateSaltKey(10);
        var encryptedNewPassword = _encryptionService.CreatePasswordHash(newPassword, saltKey);

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

    public async Task<RegistrationResultEnum> RegisterAsync(User user,
        List<int> roleIds,
        User loggedUser,
        bool emailWelcomeKit)
    {
        user.Code = Guid.NewGuid();
        user.Status = (int)UserStatusEnum.Active;
        user.LanguageId = loggedUser.LanguageId;
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

            return RegistrationResultEnum.Successful;
        }

        return RegistrationResultEnum.Failed;
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
                if (user == null || user.Id <= 0 || user.Deleted || user.Status != (int)UserStatusEnum.Active)
                    return null;

                var isValidToken = _encryptionService.ValidateToken(tokenArr[1].ToString(), 10);
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

    public async Task<IList<Project>> GetAllAccessibleProjects(int userId)
    {
        var query = _userProjectMapRepository.Table
            .Where(x => x.Project.Active && !x.Project.Deleted && x.UserId == userId)
            .Select(s => s.Project);

        return await _cacheManager.GetAsync(ServiceConstant.AccessibleProjectCacheKey, () => query.ToListAsync());
    }

    public async Task<UserProjectMap> GetProjectMapping(int userId, int projectId)
    {
        var query = _userProjectMapRepository.Table
            .Where(x => x.Project.Active && !x.Project.Deleted && x.UserId == userId && x.ProjectId == projectId);

        return await query.FirstOrDefaultAsync();
    }

    #endregion
}