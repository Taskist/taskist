using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
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
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public UserRoleController(IUserService userService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _userService = userService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER_ROLE)]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER_ROLE)]
    public async Task<IActionResult> Create()
    {
        var model = new UserRoleModel();

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER_ROLE)]
    public async Task<IActionResult> Create(UserRoleModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<UserRole>(model);
            var user = await _workContext.GetCurrentUserAsync();

            await _userService.InsertUserRoleAsync(entity);

            await _userActivityService.InsertAsync("UserRole", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER_ROLE)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _userService.GetUserRoleByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.Map<UserRoleModel>(entity);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER_ROLE)]
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
                entity = _mapper.Map(model, entity);
            }

            await _userService.UpdateUserRoleAsync(entity);

            await _userActivityService.InsertAsync("UserRole", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCodeEnum.InternalServerError : HttpStatusCodeEnum.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER_ROLE)]
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
            UserRolePermission = permissions.Select(x => _mapper.Map<UserRolePermissionModel>(x)).ToList(),
            UserRolePermissionMap = permissionMaps.Select(x => _mapper.Map<UserRolePermissionMapModel>(x)).ToList()
        };

        return PartialView(model);
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_USER_ROLE)]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _userService.GetPagedListUserRoleAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<UserRoleModel>(x)),
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
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    #endregion
}