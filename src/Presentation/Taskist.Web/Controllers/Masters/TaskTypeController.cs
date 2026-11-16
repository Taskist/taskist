using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

[CheckPermission(nameof(PermissionProvider.ManageTaskType))]
public class TaskTypeController : BaseController
{
    #region Fields

    protected readonly ITaskTypeService _taskTypeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public TaskTypeController(ITaskTypeService taskTypeService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        MastersMapper mapper)
    {
        _taskTypeService = taskTypeService;
        _localizationService = localizationService;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> Create()
    {
        var model = new TaskTypeModel
        {
            BackgroundColor = "#ffffff"
        };

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskTypeModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);
            await _taskTypeService.InsertAsync(entity);

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

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _taskTypeService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.ToModel(entity);

        if (string.IsNullOrEmpty(model.BackgroundColor))
            model.BackgroundColor = "#ffffff";
        if (string.IsNullOrEmpty(model.TextColor))
            model.BackgroundColor = "#000000";

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TaskTypeModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _taskTypeService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _taskTypeService.UpdateAsync(entity);

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

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _taskTypeService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _taskTypeService.DeleteAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
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
                GroupName = ((TaskTypeGroup)x.GroupId).ToString(),
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
