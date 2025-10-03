using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.Localization;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Localization;

namespace Taskist.Web.Controllers.Localization;

public class LanguageController : BaseController
{
    #region Fields

    protected readonly ILanguageService _languageService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public LanguageController(ILanguageService languageService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _languageService = languageService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    [CheckPermission(PermissionProvider.Configuration.MANAGE_LANGUAGE)]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_LANGUAGE)]
    public async Task<IActionResult> Create()
    {
        var model = new LanguageModel();

        return View(model);
    }

    [HttpPost, FormName("save-continue", "continueEditing")]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_LANGUAGE)]
    public async Task<IActionResult> Create(LanguageModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<Language>(model);
            await _languageService.InsertAsync(entity);

            await _userActivityService.InsertAsync("Language", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);
            return continueEditing ? RedirectToAction("Edit", new { id = entity.Id }) : RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.Failed"));

        return View(model);
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_LANGUAGE)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _languageService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<LanguageModel>(entity);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_LANGUAGE)]
    public async Task<IActionResult> Edit(LanguageModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _languageService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _languageService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("Language", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

            return RedirectToAction("Index");
        }

        return View(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_LANGUAGE)]
    public async Task<IActionResult> Delete(int id)
    {
        if (ModelState.IsValid)
        {
            var entity = await _languageService.GetByIdAsync(id);

            await _languageService.DeleteAsync(entity);

            await _userActivityService.InsertAsync("Language", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.Success,
                Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
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
    [CheckPermission(PermissionProvider.Configuration.MANAGE_LANGUAGE)]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _languageService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<LanguageModel>(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion
}