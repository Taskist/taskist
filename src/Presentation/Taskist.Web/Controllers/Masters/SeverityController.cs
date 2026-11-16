using Microsoft.AspNetCore.Mvc;
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

[CheckPermission(nameof(PermissionProvider.ManageSeverity))]
public class SeverityController : BaseController
{
    #region Fields

    protected readonly ISeverityService _severityService;
    protected readonly ILocalizationService _localizationService;
    protected readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public SeverityController(ISeverityService severityService,
        ILocalizationService localizationService,
        MastersMapper mapper)
    {
        _severityService = severityService;
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
        var model = new SeverityModel
        {
            BackgroundColor = "#ffffff"
        };

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(SeverityModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);
            await _severityService.InsertAsync(entity);

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
        var entity = await _severityService.GetByIdAsync(id);
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
    public async Task<IActionResult> Edit(SeverityModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _severityService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _severityService.UpdateAsync(entity);

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
        var entity = await _severityService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _severityService.DeleteAsync(entity);

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
                GroupName = ((SeverityGroup)x.GroupId).ToString(),
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
