using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;


public class LabelController : BaseController
{
    #region Fields

    protected readonly ILabelService _labelService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public LabelController(ILabelService labelService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _labelService = labelService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    [CheckPermission(PermissionProvider.Configuration.MANAGE_STATUS)]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_STATUS)]
    public async Task<IActionResult> Create()
    {
        var model = new LabelModel();

        return PartialView(model);
    }
    

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_STATUS)]
    public async Task<IActionResult> Create(LabelModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<Label>(model);

            await _labelService.InsertAsync(entity);
            await _userActivityService.InsertAsync("Label", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);

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

    [CheckPermission(PermissionProvider.Configuration.MANAGE_STATUS)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _labelService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<LabelModel>(entity);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_STATUS)]
    public async Task<IActionResult> Edit(LabelModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _labelService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _labelService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("Label", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

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
    [CheckPermission(PermissionProvider.Configuration.MANAGE_STATUS)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _labelService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _labelService.DeleteAsync(entity);
        await _userActivityService.InsertAsync("Label", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }
    #endregion
    #region Data

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_STATUS)]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _labelService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new LabelModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Color = x.Color,
                Active = x.Active
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion


}

