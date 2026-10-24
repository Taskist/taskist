using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Security;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Localization;

namespace Taskist.Web.Controllers.Localization;

[CheckPermission(nameof(PermissionProvider.ManageLanguage))]
public class LanguageController : BaseController
{
    #region Fields

    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly LocalizationMapper _mapper;

    #endregion

    #region Ctor

    public LanguageController(ILanguageService languageService,
        ILocalizationService localizationService,
        LocalizationMapper mapper)
    {
        _languageService = languageService;
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
        var model = new LanguageModel();

        return View(model);
    }

    [HttpPost, FormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(LanguageModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);
            await _languageService.InsertAsync(entity);

            return continueEditing ? RedirectToAction("Edit", new { id = entity.Id }) : RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.Failed"));

        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _languageService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.ToModel(entity);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(LanguageModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _languageService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _languageService.UpdateAsync(entity);

            return RedirectToAction("Index");
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (ModelState.IsValid)
        {
            var entity = await _languageService.GetByIdAsync(id);

            await _languageService.DeleteAsync(entity);

            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
            });
        }

        return Json(new JsonResponseModel
        {
            Status = ModelState.IsValid ? HttpStatusCode.InternalServerError : HttpStatusCode.ValidationError,
            Message = await _localizationService.GetResourceAsync("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _languageService.GetPagedListAsync(request.SearchValue, request.Start,
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
}
