using AutoMapper;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Service.Users;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

public class ModuleController : BaseController
{
    #region Fields

    protected readonly IModuleService _moduleService;
    protected readonly IUserService _userService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public ModuleController(IModuleService moduleService,
        IUserService userService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _moduleService = moduleService;
        _userService = userService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    public async Task<IActionResult> Index()
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageModule))
            return AccessDenied();

        return View();
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageModule))
            return AccessDeniedPartial();

        var model = new ModuleModel();
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ModuleModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageModule))
            return AccessDeniedPartial();

        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<Module>(model);
            var user = await _workContext.GetCurrentUserAsync();

            await _moduleService.InsertAsync(entity);

            await _userActivityService.InsertAsync("Module", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);

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

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageModule))
            return AccessDeniedPartial();

        var entity = await _moduleService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<ModuleModel>(entity);
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ModuleModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageModule))
            return AccessDeniedPartial();

        if (ModelState.IsValid)
        {
            var entity = await _moduleService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _moduleService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("Module", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

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

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageModule))
            return AccessDeniedPartial();

        var entity = await _moduleService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _moduleService.DeleteAsync(entity);
        await _userActivityService.InsertAsync("Module", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageModule))
            return AccessDeniedDataRead();

        var sortColumn = -1;
        var sortDirection = "asc";
        var search = string.Empty;

        if (!string.IsNullOrEmpty(Request.Form["order[0][column]"]))
        {
            sortColumn = int.Parse(Request.Form["order[0][column]"]);
        }

        if (!string.IsNullOrEmpty(Request.Form["order[0][dir]"]))
        {
            sortDirection = Request.Form["order[0][dir]"];
        }

        if (!string.IsNullOrEmpty(Request.Form["search[value]"]))
        {
            search = Request.Form["search[value]"];
        }

        var data = await _moduleService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<ModuleModel>(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    private async Task InitModelAsync(ModuleModel model)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var projects = await _userService.GetAllAccessibleProjects(loggedUser.Id);

        foreach (var item in projects)
        {
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ProjectId
            });
        }
    }

    #endregion
}