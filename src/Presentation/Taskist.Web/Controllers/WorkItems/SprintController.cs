using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.WorkItems;

public class SprintController : BaseController
{
    #region Fields

    protected readonly ISprintService _sprintService;
    protected readonly IUserService _userService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public SprintController(ISprintService sprintService,
        IUserService userService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _sprintService = sprintService;
        _userService = userService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_SPRINT)]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_SPRINT)]
    public async Task<IActionResult> Create()
    {
        var model = new SprintModel();
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_SPRINT)]
    public async Task<IActionResult> Create(SprintModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<Sprint>(model);
            var user = await _workContext.GetCurrentUserAsync();

            await _sprintService.InsertAsync(entity);

            await _userActivityService.InsertAsync("Sprint", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);

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

    [CheckPermission(PermissionProvider.WorkItem.MANAGE_SPRINT)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _sprintService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<SprintModel>(entity);
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_SPRINT)]
    public async Task<IActionResult> Edit(SprintModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _sprintService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _sprintService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("Sprint", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

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
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_SPRINT)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _sprintService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _sprintService.DeleteAsync(entity);
        await _userActivityService.InsertAsync("Sprint", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(PermissionProvider.WorkItem.MANAGE_SPRINT)]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
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

        var data = await _sprintService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<SprintModel>(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    private async Task InitModelAsync(SprintModel model)
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