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

public class SeverityController : BaseController
{
    #region Fields

    protected readonly ISeverityService _severityService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public SeverityController(ISeverityService severityService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _severityService = severityService;
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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageSeverity))
            return AccessDenied();

        return View();
    }

    public async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageSeverity))
            return AccessDeniedPartial();

        var model = new SeverityModel();

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(SeverityModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageSeverity))
            return AccessDeniedPartial();

        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<Severity>(model);
            await _severityService.InsertAsync(entity);

            await _userActivityService.InsertAsync("Severity", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);

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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageSeverity))
            return AccessDeniedPartial();

        var entity = await _severityService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<SeverityModel>(entity);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(SeverityModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageSeverity))
            return AccessDeniedPartial();

        if (ModelState.IsValid)
        {
            var entity = await _severityService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _severityService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("Severity", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageSeverity))
            return AccessDeniedPartial();

        var entity = await _severityService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _severityService.DeleteAsync(entity);
        await _userActivityService.InsertAsync("Severity", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageSeverity))
            return AccessDeniedDataRead();

        var data = await _severityService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new SeverityModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                GroupName = ((SeverityGroupEnum)x.GroupId).ToString(),
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