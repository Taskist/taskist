using Taskist.Core.Caching;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.Masters;
using Taskist.Data.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Common;
using Microsoft.EntityFrameworkCore;

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
		var query = (from menu in _menuRepository.Table
					 join rolePermission in _repositoryUserRolePermission.Table on menu.Permission equals rolePermission.SystemName
					 join rolePermissionMap in _repositoryUserRolePermissionMap.Table on rolePermission.Id equals rolePermissionMap.PermissionId
					 join userRoleMap in _repositoryUserRoleMap.Table on rolePermissionMap.UserRoleId equals userRoleMap.UserRoleId
					 where menu.Active && userRoleMap.UserId == user.Id
					 orderby menu.DisplayOrder
					 select menu).Distinct();

		return await _cacheManager.GetAsync(key, async () => await query.ToListAsync());
	}

	public async Task InsertAsync(IList<Menu> entities)
	{
		if (entities == null)
			throw new ArgumentNullException(nameof(entities));

		await _menuRepository.InsertAsync(entities);
	}

	#endregion
}