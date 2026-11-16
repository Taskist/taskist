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

public class SubModuleController : BaseController
{
    #region Fields

    protected readonly ISubModuleService _subModuleService;
    protected readonly IModuleService _moduleService;
    protected readonly IUserService _userService;
    protected readonly ILocalizationService _localizationService;
    protected readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public SubModuleController(ISubModuleService subModuleService,
        IModuleService moduleService,
        IUserService userService,
        ILocalizationService localizationService,
        MastersMapper mapper)
    {
        _subModuleService = subModuleService;
        _moduleService = moduleService;
        _userService = userService;
        _localizationService = localizationService;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    [CheckPermission(nameof(PermissionProvider.ManageSubModule))]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(nameof(PermissionProvider.ManageSubModule))]
    public async Task<IActionResult> Create()
    {
        var model = new SubModuleModel();
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageSubModule))]
    public async Task<IActionResult> Create(SubModuleModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.ToEntity(model);

            await _subModuleService.InsertAsync(entity);

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

    [CheckPermission(nameof(PermissionProvider.ManageSubModule))]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _subModuleService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.ToModel(entity);
        await InitModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageSubModule))]
    public async Task<IActionResult> Edit(SubModuleModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _subModuleService.GetByIdAsync(model.Id);
            _mapper.Update(model, entity);

            await _subModuleService.UpdateAsync(entity);

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
    [CheckPermission(nameof(PermissionProvider.ManageSubModule))]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _subModuleService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _subModuleService.DeleteAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageSubModule))]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _subModuleService.GetPagedListAsync(request.SearchValue, request.Start,
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

    #region Ajax

    public async Task<List<SelectListItem>> GetSubModules(int module)
    {
        var states = await _subModuleService.GetAllActiveByModuleAsync(module);
        return states.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }).ToList();
    }

    #endregion

    #region Helper

    private async Task InitModelAsync(SubModuleModel model)
    {
        var modules = await _moduleService.GetAllActiveAsync();
        var owners = await _userService.GetAllActiveAsync();

        foreach (var item in modules)
        {
            model.AvailableModules.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ModuleId
            });
        }

        foreach (var item in owners)
        {
            model.AvailableOwners.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.OwnerId
            });
        }
    }

    #endregion
}
