using Microsoft.EntityFrameworkCore;
using Taskist.Core.Caching;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Data.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Common;

namespace Taskist.Service.Masters;

public class MenuService : IMenuService
{
    #region Fields

    protected readonly IRepository<Menu> _menuRepository;
    protected readonly IRepository<UserRolePermission> _repositoryUserRolePermission;
    protected readonly IRepository<UserRolePermissionMap> _repositoryUserRolePermissionMap;
    protected readonly IRepository<UserRoleMap> _repositoryUserRoleMap;
    protected readonly ICacheManager _cacheManager;

    #endregion

    #region Ctor

    public MenuService(IRepository<Menu> menuRepository,
        ICacheManager cacheManager,
        IRepository<UserRolePermission> repositoryUserRolePermission,
        IRepository<UserRolePermissionMap> repositoryUserRolePermissionMap,
        IRepository<UserRoleMap> repositoryUserRoleMap)
    {
        _menuRepository = menuRepository;
        _cacheManager = cacheManager;
        _repositoryUserRolePermission = repositoryUserRolePermission;
        _repositoryUserRolePermissionMap = repositoryUserRolePermissionMap;
        _repositoryUserRoleMap = repositoryUserRoleMap;
    }

    #endregion

    #region Methods

    public async Task<IList<Menu>> GetAllAsync()
    {
        return await _menuRepository.GetAllAsync(includeDeleted: false);
    }

    public async Task<IList<Menu>> GetAllAsync(User user)
    {
        var key = string.Format(ServiceConstant.MenuCacheKeyByUser, user.Id);

        return await _cacheManager.GetAsync(key, async () =>
        {
            var permitted = await (from menu in _menuRepository.Table
                                   join rolePermission in _repositoryUserRolePermission.Table on menu.Permission equals rolePermission.SystemName
                                   join rolePermissionMap in _repositoryUserRolePermissionMap.Table on rolePermission.Id equals rolePermissionMap.PermissionId
                                   join userRoleMap in _repositoryUserRoleMap.Table on rolePermissionMap.UserRoleId equals userRoleMap.UserRoleId
                                   where menu.Active && userRoleMap.UserId == user.Id
                                   select menu).Distinct().ToListAsync();

            var parentCodes = permitted.Where(m => m.ParentCode > 0).Select(m => m.ParentCode!.Value).Distinct().ToList();
            var missingParentCodes = parentCodes.Where(c => permitted.All(m => m.Code != c)).ToList();

            if (missingParentCodes.Count > 0)
            {
                var parents = await _menuRepository.Table
                    .Where(m => m.Active && m.ParentCode == 0 && missingParentCodes.Contains(m.Code))
                    .ToListAsync();
                permitted.AddRange(parents);
            }

            return permitted.OrderBy(m => m.DisplayOrder).ToList();
        });
    }

    public async Task InsertAsync(IList<Menu> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _menuRepository.InsertAsync(entities);
    }

    #endregion
}