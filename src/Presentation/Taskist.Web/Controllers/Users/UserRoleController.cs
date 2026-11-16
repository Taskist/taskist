using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Helpers.Reports;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Users;

public class UserRoleController : BaseController
{
    #region Fields

    protected readonly IUserService _userService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _activityService;
    protected readonly IWorkContext _workContext;
    protected readonly UsersMapper _mapper;

    #endregion

    #region Ctor

    public UserRoleController(IUserService userService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService activityService,
        IWorkContext workContext,
        UsersMapper mapper)
    {
        _userService = userService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _activityService = activityService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> Create()
    {
        var model = new UserRoleModel();

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> Create(UserRoleModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);
            var user = await _workContext.GetCurrentUserAsync();

            await _userService.InsertUserRoleAsync(entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _userService.GetUserRoleByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.ToModel(entity);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> Edit(UserRoleModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _userService.GetUserRoleByIdAsync(model.Id);
            if (entity.SystemDefined)
            {
                entity.Name = model.Name;
                entity.Description = model.Description;
            }
            else
            {
                _mapper.Update(model, entity);
            }

            await _userService.UpdateUserRoleAsync(entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> Permission(int id)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var permissions = await _permissionService.GetAllUserRolePermissionsAsync();
        var permissionMaps = await _userService.GetAllUserRolePermissionMapsAsync();
        var role = await _userService.GetUserRoleByIdAsync(id);
        var model = new UserRolePermissionGridModel
        {
            RoleId = id,
            SystemRole = role.SystemDefined,
            IsAdmin = loggedUser.IsAdmin,
            UserRolePermission = permissions.Select(x => _mapper.ToModel(x)).ToList(),
            UserRolePermissionMap = permissionMaps.Select(x => _mapper.ToModel(x)).ToList()
        };

        return PartialView(model);
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _userService.GetPagedListUserRoleAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.ToModel(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Ajax

    [HttpPost]
    public async Task<bool> SetPermission(int role, int permission, bool flag)
    {
        var rolePermission = await _permissionService.GetByIdAsync(permission);
        var roles = await _userService.GetAllUserRolesAsync(true);

        if (flag)
        {
            rolePermission.UserRolePermissionMaps.Add(new UserRolePermissionMap
            {
                PermissionId = permission,
                UserRoleId = role
            });

            await _permissionService.UpdateUserRolePermissionAsync(rolePermission);
        }
        else
        {
            rolePermission.UserRolePermissionMaps
                .Remove(rolePermission.UserRolePermissionMaps.FirstOrDefault(x => x.UserRoleId == role));

            await _permissionService.UpdateUserRolePermissionAsync(rolePermission);
        }

        return true;
    }

    [HttpPost]
    public async Task<IActionResult> SavePermission(IFormCollection formCollection)
    {
        var userRoleId = Convert.ToInt32(formCollection["RoleId"]);

        await _permissionService.DeleteAllPermissionMapAsync(userRoleId);
        var permissions = await _permissionService.GetAllUserRolePermissionsAsync();

        foreach (var per in permissions)
        {
            var key = $"per_{per.Id}";
            if (formCollection.ContainsKey(key))
            {
                var rolePermission = await _permissionService.GetByIdAsync(per.Id);
                rolePermission.UserRolePermissionMaps.Add(new UserRolePermissionMap
                {
                    UserRoleId = userRoleId,
                    PermissionId = per.Id
                });

                await _permissionService.UpdateUserRolePermissionAsync(rolePermission);
            }
        }

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    #endregion

    #region Access overview

    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> Access()
    {
        var model = new UserAccessFilterModel
        {
            Roles = (await _userService.GetAllUserRolesAsync(true))
                .OrderBy(r => r.Name)
                .Select(r => new SelectListItem(r.Name, r.Id.ToString()))
                .ToList(),
            Permissions = (await _permissionService.GetAllUserRolePermissionsAsync())
                .OrderBy(p => p.Name)
                .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                .ToList()
        };

        return View(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> AccessDataRead(DataTableRequest request, string roleId, string permissionId)
    {
        var data = await BuildAccessDataAsync(ParseIds(roleId), ParseIds(permissionId));

        IEnumerable<UserAccessDataModel> query = data;
        if (!string.IsNullOrWhiteSpace(request.SearchValue))
            query = query.Where(d => d.UserName != null
                && d.UserName.Contains(request.SearchValue, StringComparison.OrdinalIgnoreCase));

        var ordered = query.OrderBy(d => d.UserName).ToList();
        var total = ordered.Count;
        var paged = request.Length <= 0 ? ordered : ordered.Skip(request.Start).Take(request.Length).ToList();

        return Json(new
        {
            request.Draw,
            data = paged.Select(d => new UserAccessRowModel
            {
                Id = d.Id,
                UserName = d.UserName,
                Roles = string.Join(", ", d.RoleNames),
                PermissionCount = d.PermissionIds.Count
            }),
            recordsFiltered = total,
            recordsTotal = total
        });
    }

    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> UserAccess(int id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NoDataPartial();

        var model = await BuildDetailAsync(user);

        return PartialView(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManageUserRole))]
    public async Task<IActionResult> ExportAccess(string roleId, string permissionId)
    {
        var data = await BuildAccessDataAsync(ParseIds(roleId), ParseIds(permissionId));
        var permissions = await _permissionService.GetAllUserRolePermissionsAsync();
        var permNameById = permissions.ToDictionary(p => p.Id, p => p.Name);

        var rows = data.OrderBy(d => d.UserName).Select(d => new UserAccessExportRowModel
        {
            User = d.UserName,
            Roles = string.Join(", ", d.RoleNames),
            Permissions = string.Join(", ", d.PermissionIds
                .Where(permNameById.ContainsKey)
                .Select(pid => permNameById[pid])
                .OrderBy(n => n))
        }).ToList();

        var bytes = ReportExcelBuilder.Build(rows,
        [
            new() { Key = "User", Title = "User" },
            new() { Key = "Roles", Title = "Roles" },
            new() { Key = "Permissions", Title = "Permissions" }
        ], "UserAccess");

        await _activityService.InsertAsync(ActivityType.Export, "Exported Roles & Permissions");

        return new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = "user-access.xlsx"
        };
    }

    #endregion

    #region Access utilities

    private static HashSet<int> ParseIds(string csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return [];

        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToHashSet();
    }

    private async Task<List<UserAccessDataModel>> BuildAccessDataAsync(HashSet<int> roleFilter, HashSet<int> permissionFilter)
    {
        var users = await _userService.GetAllActiveAsync();
        var roles = await _userService.GetAllUserRolesAsync(true);
        var roleMaps = await _userService.GetAllUserRoleMapsAsync();
        var permissionMaps = await _userService.GetAllUserRolePermissionMapsAsync();

        var roleNameById = roles.ToDictionary(r => r.Id, r => r.Name);
        var rolePermissionIds = permissionMaps
            .GroupBy(m => m.UserRoleId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.PermissionId).ToHashSet());
        var userRoleIds = roleMaps
            .GroupBy(m => m.UserId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.UserRoleId).Distinct().ToList());

        var result = new List<UserAccessDataModel>();
        foreach (var user in users)
        {
            var roleIds = (userRoleIds.TryGetValue(user.Id, out var ids) ? ids : [])
                .Where(roleNameById.ContainsKey)
                .ToList();

            var permissionIds = new HashSet<int>();
            foreach (var rid in roleIds)
                if (rolePermissionIds.TryGetValue(rid, out var ps))
                    permissionIds.UnionWith(ps);

            if (roleFilter.Count > 0 && !roleFilter.Overlaps(roleIds))
                continue;
            if (permissionFilter.Count > 0 && !permissionFilter.Overlaps(permissionIds))
                continue;

            result.Add(new UserAccessDataModel
            {
                Id = user.Id,
                UserName = user.Name,
                RoleIds = roleIds,
                RoleNames = roleIds.Select(rid => roleNameById[rid]).OrderBy(n => n).ToList(),
                PermissionIds = permissionIds
            });
        }

        return result;
    }

    private async Task<UserAccessDetailModel> BuildDetailAsync(User user)
    {
        var roles = await _userService.GetAllUserRolesAsync(true);
        var roleMaps = await _userService.GetAllUserRoleMapsAsync();
        var permissionMaps = await _userService.GetAllUserRolePermissionMapsAsync();
        var permissions = await _permissionService.GetAllUserRolePermissionsAsync();

        var roleNameById = roles.ToDictionary(r => r.Id, r => r.Name);
        var permissionById = permissions.ToDictionary(p => p.Id);
        var rolePermissionIds = permissionMaps
            .GroupBy(m => m.UserRoleId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.PermissionId).ToHashSet());

        var roleIds = roleMaps
            .Where(m => m.UserId == user.Id)
            .Select(m => m.UserRoleId)
            .Distinct()
            .Where(roleNameById.ContainsKey)
            .ToList();

        var permissionIds = new HashSet<int>();
        foreach (var rid in roleIds)
            if (rolePermissionIds.TryGetValue(rid, out var ps))
                permissionIds.UnionWith(ps);

        var groups = permissionIds
            .Where(permissionById.ContainsKey)
            .Select(pid => permissionById[pid])
            .GroupBy(p => string.IsNullOrWhiteSpace(p.RoleGroup) ? "Other" : p.RoleGroup)
            .OrderBy(g => g.Key)
            .Select(g => new UserAccessPermissionGroupModel
            {
                GroupName = g.Key,
                Permissions = g.Select(p => p.Name).OrderBy(n => n).ToList()
            })
            .ToList();

        return new UserAccessDetailModel
        {
            UserName = user.Name,
            Roles = roleIds.Select(rid => roleNameById[rid]).OrderBy(n => n).ToList(),
            PermissionGroups = groups
        };
    }

    #endregion
}
