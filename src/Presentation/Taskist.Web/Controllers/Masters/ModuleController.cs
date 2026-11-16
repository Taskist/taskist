using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Controllers.Masters;

public class ModuleController : BaseController
{
    #region Fields

    protected readonly IModuleService _moduleService;
    protected readonly IUserService _userService;
    protected readonly ILocalizationService _localizationService;
    protected readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public ModuleController(IModuleService moduleService,
        IUserService userService,
        ILocalizationService localizationService,
        MastersMapper mapper)
    {
        _moduleService = moduleService;
        _userService = userService;
        _localizationService = localizationService;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    [CheckPermission(nameof(PermissionProvider.ManageModule))]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(nameof(PermissionProvider.ManageModule))]
    public async Task<IActionResult> Create()
    {
        var model = new ModuleModel();
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageModule))]
    public async Task<IActionResult> Create(ModuleModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);

            await _moduleService.InsertAsync(entity);

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

    [CheckPermission(nameof(PermissionProvider.ManageModule))]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _moduleService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.ToModel(entity);
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageModule))]
    public async Task<IActionResult> Edit(ModuleModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _moduleService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _moduleService.UpdateAsync(entity);

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
    [CheckPermission(nameof(PermissionProvider.ManageModule))]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _moduleService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _moduleService.DeleteAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Ajax

    public async Task<List<SelectListItem>> GetModules(int project)
    {
        var states = await _moduleService.GetAllActiveByProjectAsync(project);
        return states.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }).ToList();
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageModule))]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var sortColumn = -1;
        var sortDirection = "asc";
        var search = string.Empty;

        if (int.TryParse(Request.Form["order[0][column]"], out var requestedSortColumn))
        {
            sortColumn = requestedSortColumn;
        }

        if (!string.IsNullOrEmpty(Request.Form["order[0][dir]"]))
        {
            sortDirection = Request.Form["order[0][dir]"];
        }

        if (!string.IsNullOrEmpty(Request.Form["search[value]"]))
        {
            search = Request.Form["search[value]"];
        }

        var data = await _moduleService.GetPagedListAsync(request.SearchValue, request.Start,
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

    private async Task InitModelAsync(ModuleModel model)
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
