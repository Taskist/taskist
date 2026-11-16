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

[CheckPermission(nameof(PermissionProvider.ManageLocaleResource))]
public class LocaleResourceController : BaseController
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly LocalizationMapper _mapper;

    #endregion

    #region Ctor

    public LocaleResourceController(ILocalizationService localizationService,
        LocalizationMapper mapper)
    {
        _localizationService = localizationService;
        _mapper = mapper;
    }

    #endregion

    #region Actions

    public async Task<IActionResult> Create(int languageId)
    {
        var model = new LocaleResourceModel
        {
            LanguageId = languageId
        };

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LocaleResourceModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);
            await _localizationService.InsertAsync(entity);

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
        var entity = await _localizationService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var model = _mapper.ToModel(entity);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(LocaleResourceModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _localizationService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _localizationService.UpdateAsync(entity);

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

    #endregion

    #region Data

    [HttpPost]
    public async Task<IActionResult> DataRead(DataTableRequest request, int languageId)
    {
        var data = await _localizationService.GetPagedListAsync(languageId, request.SearchValue, request.Start,
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
