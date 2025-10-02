using AutoMapper;
using Taskist.Core.Common;
using Taskist.Core.Domain.Localization;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Security;
using Microsoft.AspNetCore.Mvc;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Localization;

namespace Taskist.Web.Controllers.Localization;

public class LocaleResourceController : BaseController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IPermissionService _permissionService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public LocaleResourceController(ILocalizationService localizationService,
        IPermissionService permissionService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _localizationService = localizationService;
        _permissionService = permissionService;
        _mapper = mapper;
        _userActivityService = userActivityService;
        _workContext = workContext;
    }

    #endregion

    #region Actions

    public async Task<IActionResult> Create(int languageId)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageLocaleResource))
            return AccessDenied();

        var model = new LocaleResourceModel
        {
            LanguageId = languageId
        };

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LocaleResourceModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageLocaleResource))
            return AccessDenied();

        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<LocaleResource>(model);
            await _localizationService.InsertAsync(entity);

            await _userActivityService.InsertAsync("LocaleResource", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.ResourceValue), entity);

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
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageLocaleResource))
            return AccessDenied();

        var entity = await _localizationService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.Map<LocaleResourceModel>(entity);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(LocaleResourceModel model)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageLocaleResource))
            return AccessDenied();

        if (ModelState.IsValid)
        {
            var entity = await _localizationService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _localizationService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("LocaleResource", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.ResourceValue), entity);

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

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request, int languageId)
    {
        if (!await _permissionService.AuthorizeAsync(PermissionProvider.ManageLocaleResource))
            return AccessDeniedDataRead();

        var data = await _localizationService.GetPagedListAsync(languageId, request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<LocaleResourceModel>(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion
}