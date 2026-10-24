using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Domain.Users;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Service.Git;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Core.Domain.Settings;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Controllers.Masters;

[CheckPermission(nameof(PermissionProvider.ManageProject))]
public class ProjectController : BaseController
{
    #region Fields

    protected readonly IProjectService _projectService;
    protected readonly IClientService _clientService;
    protected readonly IUserService _userService;
    protected readonly ICustomFieldService _customFieldService;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;
    protected readonly INotificationChannelService _notificationChannelService;
    protected readonly ITaskTypeService _taskTypeService;
    protected readonly ISeverityService _severityService;
    protected readonly IStatusService _statusService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IReleaseVersionService _releaseVersionService;
    protected readonly ISettingService _settingService;
    protected readonly MastersMapper _mastersMapper;
    protected readonly UsersMapper _usersMapper;

    #endregion

    #region Ctor

    public ProjectController(IProjectService projectService,
        IClientService clientService,
        IUserService userService,
        ICustomFieldService customFieldService,
        IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService,
        INotificationChannelService notificationChannelService,
        ITaskTypeService taskTypeService,
        ISeverityService severityService,
        IStatusService statusService,
        ILocalizationService localizationService,
        IReleaseVersionService releaseVersionService,
        ISettingService settingService,
        MastersMapper mastersMapper,
        UsersMapper usersMapper)
    {
        _projectService = projectService;
        _clientService = clientService;
        _userService = userService;
        _customFieldService = customFieldService;
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _notificationChannelService = notificationChannelService;
        _taskTypeService = taskTypeService;
        _severityService = severityService;
        _statusService = statusService;
        _localizationService = localizationService;
        _releaseVersionService = releaseVersionService;
        _settingService = settingService;
        _mastersMapper = mastersMapper;
        _usersMapper = usersMapper;

    }

    #endregion

    #region Actions

    public async Task<IActionResult> Index()
    {
        return View();
    }

    public async Task<IActionResult> Create()
    {
        var model = new ProjectModel();
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost, FormName("save-continue", "continueEditing")]
    public async Task<IActionResult> Create(ProjectModel model, bool continueEditing)
    {
        if (ModelState.IsValid)
        {
            model.ReleaseVersionFormat = string.IsNullOrWhiteSpace(model.ReleaseVersionFormat) ? null : model.ReleaseVersionFormat.Trim();
            var entity = _mastersMapper.ToEntity(model);

            await _projectService.InsertAsync(entity);

            return continueEditing ? RedirectToAction("Edit", new { id = entity.Id }) : RedirectToAction("Index");
        }

        ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.Failed"));

        await InitModelAsync(model);
        return View(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _projectService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mastersMapper.ToModel(entity);
        await InitModelAsync(model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ProjectModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _projectService.GetByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("Index");

            model.ReleaseVersionFormat = string.IsNullOrWhiteSpace(model.ReleaseVersionFormat) ? null : model.ReleaseVersionFormat.Trim();
            _mastersMapper.Update(model, entity);

            await _projectService.UpdateAsync(entity);

            return RedirectToAction("Index");
        }

        await InitModelAsync(model);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _projectService.GetByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _projectService.DeleteAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Member Action

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
    public async Task<IActionResult> AddMember(UserProjectModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = new UserProjectMap
            {
                UserId = model.UserId,
                ProjectId = model.ProjectId,
                CanReportToDo = model.CanReportToDo,
                CanReportCR = model.CanReportCR,
                CanReportBug = model.CanReportBug,
                CanEdit = model.CanEdit,
                CanReOpen = model.CanReOpen,
                CanClose = model.CanClose,
                CanComment = model.CanComment,
                CanViewOthersTask = model.CanViewOthersTask,
                CanEditOthersTask = model.CanEditOthersTask,
                CanAddWiki = model.CanAddWiki,
                CanDeleteTask = model.CanDeleteTask,
                CanChangeProject = model.CanChangeProject,
                CanReviewMergeRequests = model.CanReviewMergeRequests,
                CanMergeMergeRequests = model.CanMergeMergeRequests
            };

            var user = await _userService.GetByIdAsync(model.UserId);

            await _projectService.InsertMemberAsync(entity);

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

    public async Task<IActionResult> EditMember(int id)
    {
        var entity = await _projectService.GetMemberByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _usersMapper.ToModel(entity);
        await InitMemberModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditMember(UserProjectModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _projectService.GetMemberByIdAsync(model.Id);
            _usersMapper.Update(model, entity);

            await _projectService.UpdateMemberAsync(entity);

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
    public async Task<IActionResult> DeleteMember(int id)
    {
        var entity = await _projectService.GetMemberByIdAsync(id);
        var tempEntity = entity;
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _projectService.DeleteMemberAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Custom Fields Action

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
    public async Task<IActionResult> AddCustomField(CustomFieldModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = _mastersMapper.ToEntity(model);
            await _customFieldService.InsertAsync(entity);

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

    public async Task<IActionResult> EditCustomField(int id)
    {
        var entity = await _customFieldService.GetByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = _mastersMapper.ToModel(entity);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditCustomField(CustomFieldModel model)
    {
        if (ModelState.IsValid)
        {
            var entity = await _customFieldService.GetByIdAsync(model.Id);
            _mastersMapper.Update(model, entity);

            await _customFieldService.UpdateAsync(entity);

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
    public async Task<IActionResult> DeleteCustomField(int id)
    {
        var entity = await _customFieldService.GetByIdAsync(id);
        var tempEntity = entity;
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _customFieldService.DeleteAsync(entity);

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
        var data = await _projectService.GetPagedListAsync(request.SearchValue, request.Start,
            request.Length, request.SortColumn, request.SortDirection);

        return Json(new
        {
            request.Draw,
            data = data.Select(x => _mastersMapper.ToGridModel(x)),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    [HttpPost]
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
                CanReportToDo = x.CanReportToDo,
                CanReportCR = x.CanReportCR,
                CanReportBug = x.CanReportBug,
                CanEdit = x.CanEdit,
                CanClose = x.CanClose,
                CanReOpen = x.CanReOpen,
                CanComment = x.CanComment,
                CanViewOthersTask = x.CanViewOthersTask,
                CanEditOthersTask = x.CanEditOthersTask,
                CanAddWiki = x.CanAddWiki,
                CanDeleteTask = x.CanDeleteTask,
                CanChangeProject = x.CanChangeProject,
                CanReviewMergeRequests = x.CanReviewMergeRequests,
                CanMergeMergeRequests = x.CanMergeMergeRequests
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    [HttpPost]
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
                FieldType = x.FieldType.ToString<FieldType>(),
                Mandatory = x.Mandatory,
                ColumnClass = $"{x.ColumnClass} column",
                Placement = x.Placement.ToString<CustomFieldPlacement>()
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion

    #region Helper

    [HttpGet]
    public async Task<IActionResult> ReleaseVersionPreview(int id, string? format, int? fullIncrement)
    {
        format = string.IsNullOrWhiteSpace(format) ? null : format.Trim();
        if (format != null && !ReleaseVersionTemplate.IsValid(format))
            return Json(new { valid = false, message = await _localizationService.GetResourceAsync("ProjectModel.ReleaseVersionFormat.InvalidMsg") });

        var part = fullIncrement.HasValue && Enum.IsDefined(typeof(ReleaseVersionPart), fullIncrement.Value) ? (ReleaseVersionPart?)fullIncrement.Value : null;
        var (full, patch) = await _releaseVersionService.PreviewAsync(id, format, part);

        return Json(new { valid = true, full = full.Version, patch = patch.Version, format = full.Format });
    }

    private async Task InitModelAsync(ProjectModel model)
    {
        model.DefaultReleaseVersionFormat = (await _settingService.LoadSettingAsync<WorkItemSettings>()).ReleaseVersionFormat;

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

        var projectManagers = await _userService.GetAllActiveByRoleAsync(UserConstant.ProjectManagerRoleName);

        foreach (var item in projectManagers)
        {
            model.AvailableProjectManagers.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ProjectManagerId
            });
        }

        var teamLeads = await _userService.GetAllActiveByRoleAsync(UserConstant.TeamLeadRoleName);

        foreach (var item in teamLeads)
        {
            model.AvailableTeamLeads.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.TeamLeadId
            });
        }

        foreach (var item in projectManagers.Concat(teamLeads).DistinctBy(x => x.Id).OrderBy(x => x.Name))
        {
            model.AvailableCodeReviewers.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.CodeReviewerId
            });
        }

    }

    #endregion

    #region Notifications

    [HttpPost]
    public async Task<IActionResult> DataReadNotification(int projectId, DataTableRequest request)
    {
        var eventNames = await ChatEventNamesAsync();
        var typeNames = await ChannelTypeNamesAsync();
        var channels = await _notificationChannelService.GetProjectChannelsAsync(projectId);

        return Json(new
        {
            request.Draw,
            data = channels.Select(x => new ProjectNotificationChannelGridModel
            {
                Id = x.Id,
                Channel = x.NotificationChannel.Name,
                Type = typeNames.TryGetValue(x.NotificationChannel.ChannelType, out var type) ? type : x.NotificationChannel.ChannelType.ToString(),
                Events = string.Join(", ", eventNames.Where(e => (x.Events & e.Key) != 0).Select(e => e.Value)),
                Active = x.Active && x.NotificationChannel.Active
            }),
            recordsFiltered = channels.Count,
            recordsTotal = channels.Count
        });
    }

    public async Task<IActionResult> AddNotification(int projectId)
    {
        var model = new ProjectNotificationChannelModel
        {
            ProjectId = projectId,
            Events = Enum.GetValues<ChatEvent>().Where(e => e != ChatEvent.None && e != ChatEvent.All && e != ChatEvent.TaskUpdated).Select(e => (int)e).ToList()
        };
        await InitNotificationModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddNotification(ProjectNotificationChannelModel model)
    {
        if (!ModelState.IsValid)
            return NotificationErrors();

        var error = await ValidateNotificationAsync(model, null);
        if (error != null)
            return NotificationErrors(error);

        await _notificationChannelService.InsertProjectChannelAsync(new Core.Domain.Masters.ProjectNotificationChannel
        {
            ProjectId = model.ProjectId,
            NotificationChannelId = model.NotificationChannelId,
            Events = EventFlags(model.Events),
            Active = model.Active
        });

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        });
    }

    public async Task<IActionResult> EditNotification(int id)
    {
        var entity = await _notificationChannelService.GetProjectChannelByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = new ProjectNotificationChannelModel
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            NotificationChannelId = entity.NotificationChannelId,
            Events = Enum.GetValues<ChatEvent>().Where(e => e != ChatEvent.None && e != ChatEvent.All && (entity.Events & (int)e) != 0).Select(e => (int)e).ToList(),
            Active = entity.Active
        };
        await InitNotificationModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditNotification(ProjectNotificationChannelModel model)
    {
        if (!ModelState.IsValid)
            return NotificationErrors();

        var entity = await _notificationChannelService.GetProjectChannelByIdAsync(model.Id);
        if (entity == null || entity.ProjectId != model.ProjectId)
            return NotificationErrors(await _localizationService.GetResourceAsync("FormNoData.Description"));

        var error = await ValidateNotificationAsync(model, entity);
        if (error != null)
            return NotificationErrors(error);

        entity.NotificationChannelId = model.NotificationChannelId;
        entity.Events = EventFlags(model.Events);
        entity.Active = model.Active;
        await _notificationChannelService.UpdateProjectChannelAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteNotification(int id)
    {
        var entity = await _notificationChannelService.GetProjectChannelByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _notificationChannelService.DeleteProjectChannelAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    #endregion

    #region Repositories

    [HttpPost]
    public async Task<IActionResult> DataReadRepository(int projectId, DataTableRequest request)
    {
        var syncModes = await SyncModeNamesAsync();
        var repositories = await _gitRepositoryService.GetProjectRepositoriesAsync(projectId, includeInactive: true);

        return Json(new
        {
            request.Draw,
            data = repositories.Select(x => new ProjectRepositoryGridModel
            {
                Id = x.Id,
                Name = string.IsNullOrWhiteSpace(x.DisplayName) ? x.RepoRef : x.DisplayName!,
                Connection = x.GitConnection?.Name ?? string.Empty,
                Primary = x.IsPrimary,
                Active = x.Active,
                IssueSync = syncModes.TryGetValue(x.IssueSyncMode, out var mode) ? mode : x.IssueSyncMode.ToString(),
                WebhookSecured = !string.IsNullOrWhiteSpace(x.WebhookSecret),
                LastDelivery = x.WebhookLastDeliveryOn?.ToLocalTime().ToString("dd-MMM-yyyy HH:mm")
            }),
            recordsFiltered = repositories.Count,
            recordsTotal = repositories.Count
        });
    }

    public async Task<IActionResult> AddRepository(int projectId)
    {
        var model = new ProjectRepositoryModel { ProjectId = projectId, IsPrimary = !(await _gitRepositoryService.GetProjectRepositoriesAsync(projectId, true)).Any() };
        await InitRepositoryModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddRepository(ProjectRepositoryModel model)
    {
        if (!ModelState.IsValid)
            return RepositoryErrors();

        var (repository, error) = await ResolveRemoteRepositoryAsync(model);
        if (error != null)
            return RepositoryErrors(error);

        var entity = new Core.Domain.Git.ProjectRepository
        {
            ProjectId = model.ProjectId,
            WebhookKey = _gitRepositoryService.NewWebhookKey(),
            WebhookSecret = _gitRepositoryService.NewWebhookSecret(),
            WebhookSecretCreatedOn = DateTime.UtcNow
        };
        ApplyRepository(model, entity, repository!);

        await _gitRepositoryService.InsertRepositoryAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("ProjectRepository.Added")
        });
    }

    public async Task<IActionResult> EditRepository(int id)
    {
        var entity = await _gitRepositoryService.GetRepositoryByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        var model = new ProjectRepositoryModel
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            GitConnectionId = entity.GitConnectionId,
            RepoRef = entity.RepoRef,
            RepoName = entity.DisplayName,
            IsPrimary = entity.IsPrimary,
            Active = entity.Active,
            IsPrivate = entity.IsPrivate,
            IssueSyncMode = entity.IssueSyncMode,
            IssueImportLabel = entity.IssueImportLabel,
            IssueSyncTaskTypeIds = ParseIds(entity.IssueSyncTaskTypeIds),
            IssueSyncComments = entity.IssueSyncComments,
            IssueTaskTypeId = entity.IssueTaskTypeId,
            IssueSeverityId = entity.IssueSeverityId,
            IssueOpenStatusId = entity.IssueOpenStatusId,
            IssueClosedStatusId = entity.IssueClosedStatusId,
            IssueSyncUserId = entity.IssueSyncUserId,
            IssueImportLimitPerHour = entity.IssueImportLimitPerHour > 0 ? entity.IssueImportLimitPerHour : 50
        };
        await InitRepositoryModelAsync(model);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditRepository(ProjectRepositoryModel model)
    {
        if (!ModelState.IsValid)
            return RepositoryErrors();

        var entity = await _gitRepositoryService.GetRepositoryByIdAsync(model.Id);
        if (entity == null || entity.ProjectId != model.ProjectId)
            return RepositoryErrors(await _localizationService.GetResourceAsync("FormNoData.Description"));

        var (repository, error) = await ResolveRemoteRepositoryAsync(model);
        if (error != null)
            return RepositoryErrors(error);

        ApplyRepository(model, entity, repository!);
        await _gitRepositoryService.UpdateRepositoryAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.UpdateSuccess")
        });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteRepository(int id)
    {
        var entity = await _gitRepositoryService.GetRepositoryByIdAsync(id);
        if (entity == null)
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            });

        await _gitRepositoryService.DeleteRepositoryAsync(entity);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        });
    }

    public async Task<IActionResult> RepositoryWebhook(int id)
    {
        var entity = await _gitRepositoryService.GetRepositoryByIdAsync(id);
        if (entity == null)
            return NoDataPartial();

        return PartialView(new RepositoryWebhookModel
        {
            RepositoryId = entity.Id,
            RepositoryName = string.IsNullOrWhiteSpace(entity.DisplayName) ? entity.RepoRef : entity.DisplayName!,
            ProviderType = entity.GitConnection.ProviderType,
            WebhookUrl = WebhookUrl(entity.WebhookKey),
            HasSecret = !string.IsNullOrWhiteSpace(entity.WebhookSecret),
            SecretCreatedOn = entity.WebhookSecretCreatedOn,
            LastDeliveryOn = entity.WebhookLastDeliveryOn,
            IssueEvents = entity.IssueSyncMode != (int)IssueSyncMode.Off
        });
    }

    [HttpPost]
    public async Task<IActionResult> RegenerateWebhookSecret(int id)
    {
        var entity = await _gitRepositoryService.GetRepositoryByIdAsync(id);
        if (entity == null)
            return Json(new { status = HttpStatusCode.NoData });

        var secret = _gitRepositoryService.NewWebhookSecret();
        entity.WebhookSecret = secret;
        entity.WebhookSecretCreatedOn = DateTime.UtcNow;
        await _gitRepositoryService.UpdateRepositoryAsync(entity);

        Response.Headers.CacheControl = "no-store";
        return Json(new { status = HttpStatusCode.Success, secret, url = WebhookUrl(entity.WebhookKey) });
    }

    [HttpPost]
    public async Task<IActionResult> RegisterWebhook(int id)
    {
        var entity = await _gitRepositoryService.GetRepositoryByIdAsync(id);
        var context = await _gitRepositoryService.GetContextAsync(entity);
        if (entity == null || context == null)
            return Json(new { status = HttpStatusCode.NoData, message = await _localizationService.GetResourceAsync("FormNoData.Description") });

        var user = await _userService.GetByIdAsync(UserId);
        var (token, error) = await _gitAccountService.RequireTokenAsync(user, context.Connection, "register webhooks");
        if (error != null)
            return Json(new { status = HttpStatusCode.ValidationError, message = error });

        var secret = _gitRepositoryService.NewWebhookSecret();
        var result = await context.Provider.RegisterWebhookAsync(context.RepoRef, new Core.Dtos.Git.GitWebhookRegistrationDto
        {
            Url = WebhookUrl(entity.WebhookKey),
            Secret = secret,
            IssueEvents = entity.IssueSyncMode != (int)IssueSyncMode.Off
        }, token!);
        if (!result.Success)
            return Json(new { status = HttpStatusCode.ValidationError, message = result.Error });

        entity.WebhookSecret = secret;
        entity.WebhookSecretCreatedOn = DateTime.UtcNow;
        await _gitRepositoryService.UpdateRepositoryAsync(entity);

        return Json(new { status = HttpStatusCode.Success, message = await _localizationService.GetResourceAsync("ProjectRepository.WebhookRegistered") });
    }

    public async Task<JsonResult> SearchGitRepositories(int connectionId, string? q)
    {
        var connection = await _gitRepositoryService.GetConnectionByIdAsync(connectionId);
        if (connection == null)
            return Json(Array.Empty<SelectListItem>());

        var provider = _gitRepositoryService.ProviderFor(connection);
        var repositories = await provider.SearchRepositoriesAsync(q);
        if (repositories.Count == 0 && !string.IsNullOrEmpty(provider.LastError))
            return Json(new { error = provider.LastError });

        return Json(repositories.Select(p => new SelectListItem
        {
            Value = connection.ProviderType == (int)GitProviderType.GitHub ? p.PathWithNamespace : p.Id.ToString(),
            Text = string.IsNullOrWhiteSpace(p.PathWithNamespace) ? p.Name : p.PathWithNamespace
        }));
    }

    private async Task<(Core.Dtos.Git.GitRepositoryDto? Repository, string? Error)> ResolveRemoteRepositoryAsync(ProjectRepositoryModel model)
    {
        var connection = await _gitRepositoryService.GetConnectionByIdAsync(model.GitConnectionId);
        if (connection == null || !connection.Active)
            return (null, await _localizationService.GetResourceAsync("ProjectRepositoryModel.GitConnectionId.RequiredMsg"));

        var provider = _gitRepositoryService.ProviderFor(connection);
        var repoRef = model.RepoRef?.Trim() ?? string.Empty;
        if (!provider.IsValidRepoRef(repoRef))
            return (null, await _localizationService.GetResourceAsync("ProjectRepositoryModel.RepoRef.InvalidMsg"));

        var duplicate = (await _gitRepositoryService.GetProjectRepositoriesAsync(model.ProjectId, true))
            .Any(x => x.Id != model.Id && x.GitConnectionId == connection.Id && string.Equals(x.RepoRef, repoRef, StringComparison.OrdinalIgnoreCase));
        if (duplicate)
            return (null, await _localizationService.GetResourceAsync("ProjectRepositoryModel.RepoRef.DuplicateMsg"));

        var repository = await provider.GetRepositoryAsync(repoRef);
        return repository == null
            ? (null, provider.LastError ?? await _localizationService.GetResourceAsync("ProjectRepositoryModel.RepoRef.NotFoundMsg"))
            : (repository, null);
    }

    private static void ApplyRepository(ProjectRepositoryModel model, Core.Domain.Git.ProjectRepository entity, Core.Dtos.Git.GitRepositoryDto repository)
    {
        var wasOff = entity.IssueSyncMode == (int)IssueSyncMode.Off;

        entity.GitConnectionId = model.GitConnectionId;
        entity.RepoRef = model.RepoRef!.Trim();
        entity.ExternalRepoId = repository.Id > 0 ? repository.Id.ToString() : entity.ExternalRepoId;
        entity.DisplayName = string.IsNullOrWhiteSpace(repository.PathWithNamespace) ? repository.Name : repository.PathWithNamespace;
        entity.WebUrl = repository.WebUrl;
        entity.DefaultBranch = repository.DefaultBranch;
        entity.IsPrivate = repository.IsPrivate;
        entity.IsPrimary = model.IsPrimary;
        entity.Active = model.Active;
        entity.IssueSyncMode = model.IssueSyncMode;
        entity.IssueImportLabel = string.IsNullOrWhiteSpace(model.IssueImportLabel) ? null : model.IssueImportLabel.Trim();
        entity.IssueSyncTaskTypeIds = model.IssueSyncTaskTypeIds.Count == 0 ? null : string.Join(",", model.IssueSyncTaskTypeIds.Distinct());
        entity.IssueSyncComments = model.IssueSyncComments;
        entity.IssueTaskTypeId = model.IssueTaskTypeId > 0 ? model.IssueTaskTypeId : null;
        entity.IssueSeverityId = model.IssueSeverityId > 0 ? model.IssueSeverityId : null;
        entity.IssueOpenStatusId = model.IssueOpenStatusId > 0 ? model.IssueOpenStatusId : null;
        entity.IssueClosedStatusId = model.IssueClosedStatusId > 0 ? model.IssueClosedStatusId : null;
        entity.IssueSyncUserId = model.IssueSyncUserId > 0 ? model.IssueSyncUserId : null;
        entity.IssueImportLimitPerHour = Math.Clamp(model.IssueImportLimitPerHour, 1, 1000);

        if (wasOff && entity.IssueSyncMode != (int)IssueSyncMode.Off)
        {
            entity.IssueSyncEnabledOn = DateTime.UtcNow;
            entity.IssueLastPolledOn = DateTime.UtcNow;
        }
    }

    private async Task InitRepositoryModelAsync(ProjectRepositoryModel model)
    {
        var connections = await _gitRepositoryService.GetConnectionsAsync();
        if (model.GitConnectionId <= 0)
            model.GitConnectionId = connections.FirstOrDefault()?.Id ?? 0;

        foreach (var connection in connections)
            model.AvailableConnections.Add(new SelectListItem { Text = connection.Name, Value = connection.Id.ToString(), Selected = connection.Id == model.GitConnectionId });

        foreach (var (value, text) in await SyncModeNamesAsync())
            model.AvailableSyncModes.Add(new SelectListItem { Text = text, Value = value.ToString(), Selected = value == model.IssueSyncMode });

        foreach (var type in await _taskTypeService.GetAllActiveAsync())
            model.AvailableTaskTypes.Add(new SelectListItem { Text = type.Name, Value = type.Id.ToString() });

        foreach (var severity in await _severityService.GetAllActiveAsync())
            model.AvailableSeverities.Add(new SelectListItem { Text = severity.Name, Value = severity.Id.ToString() });

        foreach (var status in await _statusService.GetAllActiveByProjectAsync(model.ProjectId))
            model.AvailableStatuses.Add(new SelectListItem { Text = status.Name, Value = status.Id.ToString() });

        foreach (var user in await _userService.GetAllActiveAsync())
            model.AvailableUsers.Add(new SelectListItem { Text = user.Name, Value = user.Id.ToString() });
    }

    private async Task<Dictionary<int, string>> SyncModeNamesAsync()
    {
        var result = new Dictionary<int, string>();
        foreach (var mode in Enum.GetValues<IssueSyncMode>())
            result[(int)mode] = await _localizationService.GetResourceAsync($"IssueSyncMode.{mode}");
        return result;
    }

    private JsonResult RepositoryErrors(string? message = null)
    {
        if (!string.IsNullOrEmpty(message))
            ModelState.AddModelError(string.Empty, message);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.ValidationError,
            Message = message ?? _localizationService.GetResource("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    private string WebhookUrl(string key) => $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/git/hooks/{key}";

    private static List<int> ParseIds(string? csv) =>
        (csv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => int.TryParse(x, out var id) ? id : 0).Where(x => x > 0).ToList();

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

    #region Notification utilities

    private async Task InitNotificationModelAsync(ProjectNotificationChannelModel model)
    {
        var typeNames = await ChannelTypeNamesAsync();
        var channels = (await _notificationChannelService.GetChannelsAsync(false))
            .Where(x => x.Active || x.Id == model.NotificationChannelId);
        foreach (var channel in channels)
            model.AvailableChannels.Add(new SelectListItem
            {
                Text = $"{channel.Name} ({(typeNames.TryGetValue(channel.ChannelType, out var type) ? type : channel.ChannelType.ToString())})",
                Value = channel.Id.ToString(),
                Selected = channel.Id == model.NotificationChannelId
            });

        foreach (var item in await ChatEventNamesAsync())
            model.AvailableEvents.Add(new SelectListItem { Text = item.Value, Value = item.Key.ToString(), Selected = model.Events.Contains(item.Key) });
    }

    private async Task<string?> ValidateNotificationAsync(ProjectNotificationChannelModel model, Core.Domain.Masters.ProjectNotificationChannel? entity)
    {
        var channel = await _notificationChannelService.GetChannelByIdAsync(model.NotificationChannelId);
        if (channel == null || (!channel.Active && entity?.NotificationChannelId != channel.Id))
            return await _localizationService.GetResourceAsync("ProjectNotificationChannelModel.NotificationChannelId.RequiredMsg");

        if (await _notificationChannelService.ProjectChannelExistsAsync(model.ProjectId, model.NotificationChannelId, entity?.Id ?? 0))
            return await _localizationService.GetResourceAsync("ProjectNotificationChannel.Duplicate");

        return null;
    }

    private static int EventFlags(IEnumerable<int> events)
    {
        var valid = (int)ChatEvent.All;
        return events.Aggregate(0, (flags, value) => flags | (value & valid));
    }

    private async Task<Dictionary<int, string>> ChatEventNamesAsync()
    {
        var names = new Dictionary<int, string>();
        foreach (var value in Enum.GetValues<ChatEvent>().Where(e => e != ChatEvent.None && e != ChatEvent.All))
            names[(int)value] = await _localizationService.GetResourceAsync($"ChatEvent.{value}");
        return names;
    }

    private async Task<Dictionary<int, string>> ChannelTypeNamesAsync()
    {
        var names = new Dictionary<int, string>();
        foreach (var value in Enum.GetValues<NotificationChannelType>())
            names[(int)value] = await _localizationService.GetResourceAsync($"NotificationChannelType.{value}");
        return names;
    }

    private JsonResult NotificationErrors(string? message = null)
    {
        if (!string.IsNullOrEmpty(message))
            ModelState.AddModelError(string.Empty, message);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.ValidationError,
            Message = message ?? _localizationService.GetResource("Error.Failed"),
            Errors = ModelState.AllErrors()
        });
    }

    #endregion
}
