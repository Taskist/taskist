using AutoMapper;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Microsoft.AspNetCore.Mvc;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

public class TaskTypeController : BaseController
{
    #region Fields

    protected readonly ITaskTypeService _taskTypeService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public TaskTypeController(ITaskTypeService taskTypeService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _taskTypeService = taskTypeService;
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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageTaskType))
            return AccessDenied();

        return View();
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageTaskType))
            return AccessDeniedPartial();

        var model = new TaskTypeModel();

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskTypeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageTaskType))
            return AccessDeniedPartial();

        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<TaskType>(model);
            await _taskTypeService.InsertAsync(entity);

            await _userActivityService.InsertAsync("TaskType", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);

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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageTaskType))
            return AccessDeniedPartial();

        var entity = await _taskTypeService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<TaskTypeModel>(entity);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TaskTypeModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageTaskType))
            return AccessDeniedPartial();

        if (ModelState.IsValid)
        {
            var entity = await _taskTypeService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _taskTypeService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("TaskType", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageTaskType))
            return AccessDeniedPartial();

        var entity = await _taskTypeService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _taskTypeService.DeleteAsync(entity);
        await _userActivityService.InsertAsync("TaskType", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageTaskType))
            return AccessDeniedDataRead();

        var data = await _taskTypeService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new TaskTypeModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                GroupName = ((TaskTypeGroupEnum)x.GroupId).ToString(),
                TextColor = x.TextColor,
                BackgroundColor = x.BackgroundColor,
                IconClass = x.IconClass,
                Active = x.Active
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion
}