using Microsoft.EntityFrameworkCore;
using Taskist.Core.Caching;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Data.Repository;
using Taskist.Service.Common;
using Taskist.Service.Users;

namespace Taskist.Service.Security;

public class PermissionService : IPermissionService
{
    #region Fields

    protected readonly ICacheManager _cacheManager;
    protected readonly IUserService _userService;
    protected readonly IRepository<UserRolePermission> _userRolePermissionRepository;
    protected readonly IRepository<UserRolePermissionMap> _userRolePermissionMapRepository;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public PermissionService(ICacheManager cacheManager,
        IUserService userService,
        IRepository<UserRolePermission> permissionRecordRepository,
        IRepository<UserRolePermissionMap> userRolePermissionMapRepository,
        IWorkContext workContext)
    {
        _cacheManager = cacheManager;
        _userService = userService;
        _userRolePermissionRepository = permissionRecordRepository;
        _userRolePermissionMapRepository = userRolePermissionMapRepository;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected async Task<IList<UserRolePermission>> GetUserRolePermissionsByUserRoleIdAsync(int userRoleId)
    {
        var key = string.Format(ServiceConstant.PermissionsAllByUserRoleIdCacheKey, userRoleId);
        var query = from pr in _userRolePermissionRepository.Table
                    join urpmap in _userRolePermissionMapRepository.Table on pr.Id equals urpmap
                        .PermissionId
                    where urpmap.UserRoleId == userRoleId
                    orderby pr.Id
                    select pr;

        return await _cacheManager.GetAsync(key, async () => await query.ToListAsync());
    }

    protected async Task<bool> AuthorizeAsync(string userRoleSystemName, int userRoleId)
    {
        if (string.IsNullOrEmpty(userRoleSystemName))
            return false;

        var key = string.Format(ServiceConstant.PermissionsAllowedCacheKey, userRoleId,
            userRoleSystemName);

        return await _cacheManager.GetAsync(key, async () =>
        {
            var permissions = await GetUserRolePermissionsByUserRoleIdAsync(userRoleId);
            foreach (var permission1 in permissions)
                if (permission1.SystemName.Equals(userRoleSystemName,
                    StringComparison.InvariantCultureIgnoreCase))
                    return true;

            return false;
        });
    }

    #endregion

    #region Methods

    public async Task DeleteUserRolePermissionAsync(UserRolePermission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        await _userRolePermissionRepository.DeleteAsync(permission);

        await _cacheManager.RemoveByPrefixAsync(ServiceConstant.PermissionsPrefixCacheKey);
    }

    public async Task DeleteAllPermissionMapAsync(int userRoleId)
    {
        var all = await _userRolePermissionMapRepository.GetAllAsync(query => query.Where(x => x.UserRoleId == userRoleId));
        await _userRolePermissionMapRepository.DeleteAsync(all);

        await _cacheManager.RemoveByPrefixAsync(ServiceConstant.PermissionsPrefixCacheKey);
    }

    public async Task<UserRolePermission> GetByIdAsync(int permissionId)
    {
        if (permissionId == 0)
            return null;

        return await _userRolePermissionRepository.GetByIdAsync(permissionId);
    }

    public async Task<UserRolePermission> GetBySystemNameAsync(string systemName)
    {
        if (string.IsNullOrWhiteSpace(systemName))
            return null;

        var query = from pr in _userRolePermissionRepository.Table
                    where pr.SystemName == systemName
                    orderby pr.Id
                    select pr;

        return await query.FirstOrDefaultAsync();
        ;
    }

    public async Task<IList<UserRolePermission>> GetAllUserRolePermissionsAsync()
    {
        var query = from pr in _userRolePermissionRepository.Table
                    orderby pr.Name
                    select pr;

        return await query.ToListAsync();
    }

    public async Task InsertUserRolePermissionAsync(UserRolePermission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        await _userRolePermissionRepository.InsertAsync(permission);

        await _cacheManager.RemoveByPrefixAsync(ServiceConstant.PermissionsPrefixCacheKey);
    }

    public async Task InsertUserRolePermissionAsync(IList<UserRolePermission> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _userRolePermissionRepository.InsertAsync(entities);

        await _cacheManager.RemoveByPrefixAsync(ServiceConstant.PermissionsPrefixCacheKey);
    }

    public async Task UpdateUserRolePermissionAsync(UserRolePermission permission)
    {
        if (permission == null)
            throw new ArgumentNullException(nameof(permission));

        await _userRolePermissionRepository.UpdateAsync(permission);

        await _cacheManager.RemoveByPrefixAsync(ServiceConstant.PermissionsPrefixCacheKey);
    }

    public async Task<IList<UserRolePermission>> GetByRoleAsync(string roleName)
    {
        var query = _userRolePermissionRepository.Table.Where(x =>
            x.UserRolePermissionMaps.Any(p => p.UserRole.SystemName == roleName));

        return await query.ToListAsync();
    }

    public async Task<IList<UserRolePermission>> GetByRoleIdAsync(int userRoleId)
    {
        var query = from pr in _userRolePermissionRepository.Table
                    join urpmap in _userRolePermissionMapRepository.Table on pr.Id equals urpmap
                        .PermissionId
                    where urpmap.UserRoleId == userRoleId
                    orderby pr.Id
                    select pr;

        return await query.ToListAsync();
    }

    public async Task<bool> AuthorizeAsync(UserRolePermission permission)
    {
        return await AuthorizeAsync(permission, await _workContext.GetCurrentUserAsync());
    }

    public async Task<bool> AuthorizeAsync(UserRolePermission permission, User user)
    {
        if (permission == null)
            return false;

        if (user == null)
            return false;

        return await AuthorizeAsync(permission.SystemName, user);
    }

    public async Task<bool> AuthorizeAsync(string userRoleSystemName)
    {
        if (string.IsNullOrEmpty(userRoleSystemName))
            return false;

        var user = await _workContext.GetCurrentUserAsync();

        if (user == null)
            return false;

        return await AuthorizeAsync(userRoleSystemName, user);
    }

    public async Task<bool> AuthorizeAsync(string userRoleSystemName, User user)
    {
        if (string.IsNullOrEmpty(userRoleSystemName))
            return false;

        var customerRoles = await _userService.GetUserRolesAsync(user);
        foreach (var role in customerRoles)
            if (await AuthorizeAsync(userRoleSystemName, role.Id))
                return true;

        return false;
    }

    #endregion
}