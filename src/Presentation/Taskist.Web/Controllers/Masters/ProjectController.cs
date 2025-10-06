using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Extensions;
using Taskist.Service.Localization;
using Taskist.Service.Logging;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Masters;

public class ProjectController : BaseController
{
    #region Fields

    protected readonly IProjectService _projectService;
    protected readonly IClientService _clientService;
    protected readonly IUserService _userService;
    protected readonly ICustomFieldService _customFieldService;
    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IUserActivityService _userActivityService;
    protected readonly IWorkContext _workContext;
    protected readonly IMapper _mapper;

    #endregion

    #region Ctor

    public ProjectController(IProjectService projectService,
        IClientService clientService,
        IUserService userService,
        ICustomFieldService customFieldService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IUserActivityService userActivityService,
        IWorkContext workContext,
        IMapper mapper)
    {
        _projectService = projectService;
        _clientService = clientService;
        _userService = userService;
        _customFieldService = customFieldService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _userActivityService = userActivityService;
        _workContext = workContext;
        _mapper = mapper;

    }

    #endregion

    #region Actions

    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> Index()
    {
        return View();
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> Create()
    {
        var model = new ProjectModel();
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost, FormName("save-continue", "continueEditing")]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> Create(ProjectModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<Project>(model);
            var user = await _workContext.GetCurrentUserAsync();

            await _projectService.InsertAsync(entity);

            await _userActivityService.InsertAsync("Project", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Name), entity);
            return continueEditing ? RedirectToAction("Create") : RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.Failed"));

        await InitModelAsync(model);
        return View(model);
    }

    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _projectService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<ProjectModel>(entity);
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> Edit(ProjectModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _projectService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _projectService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("Project", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Name), entity);

            return RedirectToAction("Index");
        }

        return View(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _projectService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _projectService.DeleteAsync(entity);
        await _userActivityService.InsertAsync("Project", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), entity.Name), entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Member Action

    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> AddMember(int projectId)
    {
        var model = new UserProjectModel
        {
            ProjectId = projectId
        };

        await InitMemberModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> AddMember(UserProjectModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = new UserProjectMap
            {
                UserId = model.UserId,
                ProjectId = model.ProjectId,
                CanReport = model.CanReport,
                CanEdit = model.CanEdit,
                CanReOpen = model.CanReOpen,
                CanClose = model.CanClose,
                CanComment = model.CanComment,
                CanViewOthersTask = model.CanViewOthersTask,
                CanEditOthersTask = model.CanEditOthersTask
            };

            var user = await _userService.GetByIdAsync(model.UserId);

            await _projectService.InsertMemberAsync(entity);

            await _userActivityService.InsertAsync("ProjectMember", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), user.Name), entity);

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

    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> EditMember(int id)
    {
        var entity = await _projectService.GetMemberByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<UserProjectModel>(entity);
        await InitMemberModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> EditMember(UserProjectModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _projectService.GetMemberByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _projectService.UpdateMemberAsync(entity);

            await _userActivityService.InsertAsync("ProjectMember", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.User.Name), entity);

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
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> DeleteMember(int id)
    {
        var entity = await _projectService.GetMemberByIdAsync(id);
        var tempEntity = entity;
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _projectService.DeleteMemberAsync(entity);
        await _userActivityService.InsertAsync("ProjectMember", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), tempEntity.User.Name), tempEntity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Custom Fields Action

    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> AddCustomField(int projectId)
    {
        var project = await _projectService.GetByIdAsync(projectId);

        var model = new CustomFieldModel
        {
            ProjectId = projectId,
            ProjectName = project.Name
        };

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> AddCustomField(CustomFieldModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mapper.Map<CustomField>(model);
            await _customFieldService.InsertAsync(entity);

            await _userActivityService.InsertAsync("ProjectCustomField", string.Format(await _localizationService.GetResourceAsync("Log.RecordCreated"), entity.Label), entity);

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

    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> EditCustomField(int id)
    {
        var entity = await _customFieldService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mapper.Map<CustomFieldModel>(entity);

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> EditCustomField(CustomFieldModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _customFieldService.GetByIdAsync(model.Id);
            entity = _mapper.Map(model, entity);

            await _customFieldService.UpdateAsync(entity);

            await _userActivityService.InsertAsync("ProjectCustomField", string.Format(await _localizationService.GetResourceAsync("Log.RecordUpdated"), entity.Label), entity);

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
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> DeleteCustomField(int id)
    {
        var entity = await _customFieldService.GetByIdAsync(id);
        var tempEntity = entity;
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCodeEnum.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _customFieldService.DeleteAsync(entity);
        await _userActivityService.InsertAsync("ProjectMember", string.Format(await _localizationService.GetResourceAsync("Log.RecordDeleted"), tempEntity.Label), tempEntity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCodeEnum.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> DataRead(DataTableRequest request)
    {
        var data = await _projectService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mapper.Map<ProjectGridModel>(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> DataReadMember(int projectId, DataTableRequest request)
    {
        var data = await _projectService.GetPagedListMembersAsync(projectId, request.SearchValue, request.Start,
            request.Length);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new UserProjectGridModel
            {
                Id = x.Id,
                UserName = x.User.Name,
                CanReport = x.CanReport,
                CanEdit = x.CanEdit,
                CanClose = x.CanClose,
                CanReOpen = x.CanReOpen,
                CanComment = x.CanComment
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    [HttpPost]
    [CheckPermission(PermissionProvider.Configuration.MANAGE_PROJECT)]
    public async Task<IActionResult> DataReadCustomField(int projectId, DataTableRequest request)
    {
        var data = await _customFieldService.GetPagedListAsync(projectId, request.SearchValue, request.Start,
            request.Length);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new CustomFieldGridModel
            {
                Id = x.Id,
                Label = x.Label,
                ResourceKey = x.ResourceKey,
                FieldType = x.FieldType.ToString<FieldTypeEnum>(),
                Mandatory = x.Mandatory,
                ColumnClass = $"{x.ColumnClass} column",
                Placement = x.Placement.ToString<CustomFieldPlacementEnum>()
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    private async Task InitModelAsync(ProjectModel model)
    {
        var clients = await _clientService.GetAllActiveAsync();

        foreach (var item in clients)
        {
            model.AvailableClients.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ClientId
            });
        }
    }

    private async Task InitMemberModelAsync(UserProjectModel model)
    {
        var users = await _userService.GetAllActiveAsync();

        foreach (var item in users)
        {
            model.AvailableUsers.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.UserId
            });
        }
    }

    #endregion
}