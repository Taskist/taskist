using Taskist.Core.Enums;
using Taskist.Web.Helpers.Mapping;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Controllers.Masters;

[CheckPermission(nameof(PermissionProvider.ManageReporter))]
public class ReporterController : BaseController
{
    #region Fields

    protected readonly IReporterService _reporterService;
    protected readonly IUserService _userService;
    protected readonly ILocalizationService _localizationService;
    protected readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public ReporterController(IReporterService reporterService,
        IUserService userService,
        ILocalizationService localizationService,
        MastersMapper mapper)
    {
        _reporterService = reporterService;
        _userService = userService;
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
        var model = new ReporterModel();
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(ReporterModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);

            await _reporterService.InsertAsync(entity);

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
        var entity = await _reporterService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.ToModel(entity);
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ReporterModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _reporterService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _reporterService.UpdateAsync(entity);

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
        var entity = await _reporterService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _reporterService.DeleteAsync(entity);

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
        var data = await _reporterService.GetPagedListAsync(request.SearchValue, request.Start,
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

    #region Helper

    private async Task InitModelAsync(ReporterModel model)
    {
        var projects = await _userService.GetAllAccessibleProjects(UserId);

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
