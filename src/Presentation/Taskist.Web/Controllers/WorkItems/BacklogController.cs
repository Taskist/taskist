using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.WorkItems;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Service.Git;
using Taskist.Service.Git.Issues;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Common;
using Taskist.Web.Helpers.Documents;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Helpers.Reports;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.WorkItems;

public class BacklogController : BaseController
{
    #region Fields

    protected readonly IBacklogService _backlogItemService;
    protected readonly IUserService _userService;
    protected readonly IProjectService _projectService;
    protected readonly ITaskTypeService _taskTypeService;
    protected readonly IReporterService _reporterService;
    protected readonly ISeverityService _severityService;
    protected readonly IModuleService _moduleService;
    protected readonly ISubModuleService _subModuleService;
    protected readonly IStatusService _statusService;
    protected readonly IReleaseService _releaseService;
    protected readonly ITagService _tagService;
    protected readonly IDocumentService _documentService;
    protected readonly ICustomFieldService _customFieldService;
    protected readonly IMessageService _messageService;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;
    protected readonly IIssueSyncService _issueSyncService;
    protected readonly IGitJobScheduler _gitJobScheduler;
    protected readonly INotificationChannelService _notificationChannelService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IStatusDurationMetricService _stageMetricService;
    protected readonly IWorkContext _workContext;
    protected readonly WorkItemSettings _workItemSettings;
    protected readonly IReportLayoutService _reportLayoutService;
    protected readonly WorkItemsMapper _workItemsMapper;
    protected readonly MastersMapper _mastersMapper;
    protected readonly IPermissionService _permissionService;
    protected readonly ITaskCodeService _taskCodeService;
    protected readonly IGitBranchService _gitBranchService;
    protected readonly IMergeRequestReviewService _mergeRequestReviewService;
    protected readonly IClosingRequirementService _closingRequirementService;
    protected readonly IUploadTokenService _uploadTokenService;

    private const int BulkMoveLimit = 100;

    #endregion

    #region Ctor

    public BacklogController(IBacklogService backlogItemService,
            IUserService userService,
            IProjectService projectService,
            ITaskTypeService taskTypeService,
            IReporterService reporterService,
            ISeverityService severityService,
            IModuleService moduleService,
            ISubModuleService subModuleService,
            IStatusService statusService,
            IReleaseService releaseService,
            ITagService tagService,
            IDocumentService documentService,
            ICustomFieldService customFieldService,
            IMessageService messageService,
            IGitRepositoryService gitRepositoryService,
            IGitAccountService gitAccountService,
            IIssueSyncService issueSyncService,
            IGitJobScheduler gitJobScheduler,
            INotificationChannelService notificationChannelService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            IStatusDurationMetricService stageMetricService,
            IWorkContext workContext,
            WorkItemSettings workItemSettings,
            IReportLayoutService reportLayoutService,
            WorkItemsMapper workItemsMapper,
            MastersMapper mastersMapper,
            IPermissionService permissionService,
            ITaskCodeService taskCodeService,
            IGitBranchService gitBranchService,
            IClosingRequirementService closingRequirementService,
            IMergeRequestReviewService mergeRequestReviewService,
            IUploadTokenService uploadTokenService)
    {
        _uploadTokenService = uploadTokenService;
        _closingRequirementService = closingRequirementService;
        _mergeRequestReviewService = mergeRequestReviewService;
        _backlogItemService = backlogItemService;
        _userService = userService;
        _projectService = projectService;
        _taskTypeService = taskTypeService;
        _reporterService = reporterService;
        _severityService = severityService;
        _moduleService = moduleService;
        _subModuleService = subModuleService;
        _statusService = statusService;
        _releaseService = releaseService;
        _tagService = tagService;
        _documentService = documentService;
        _customFieldService = customFieldService;
        _messageService = messageService;
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
        _issueSyncService = issueSyncService;
        _gitJobScheduler = gitJobScheduler;
        _notificationChannelService = notificationChannelService;
        _localizationService = localizationService;
        _genericAttributeService = genericAttributeService;
        _stageMetricService = stageMetricService;
        _workContext = workContext;
        _workItemSettings = workItemSettings;
        _reportLayoutService = reportLayoutService;
        _workItemsMapper = workItemsMapper;
        _mastersMapper = mastersMapper;
        _permissionService = permissionService;
        _taskCodeService = taskCodeService;
        _gitBranchService = gitBranchService;
    }

    #endregion

    #region Actions

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Index(int filter = 0, string? filterValue = null, int project = 0, int assignee = 0)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();

        var model = await GetProjectAccess(await ResolveProjectIdAsync(0));
        model.UserGuid = loggedUser.Code.ToString("N");
        model.CanCreateBranch = _gitBranchService.CreationEnabled;
        model.FilterMode = filter;
        model.FilterValue = filterValue;
        model.FilterLabel = filterValue;
        if (filter == 4 && int.TryParse(filterValue, out var filterReleaseId))
            model.FilterLabel = (await _releaseService.GetByIdAsync(filterReleaseId))?.Name ?? filterValue;
        model.FilterProjectId = project;
        model.FilterAssigneeId = assignee;

        return View(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Create(int project = 0)
    {
        var projectId = await ResolveProjectIdAsync(project);

        var projectAccess = await GetProjectAccess(projectId);
        if (projectAccess == null || !projectAccess.CanReport && !projectAccess.CanEdit && !projectAccess.CanClose)
            return AccessDenied();

        var model = new BacklogModel { ProjectId = projectId };

        if (TempData["WikiPrefillTitle"] != null)
        {
            model.Title = TempData["WikiPrefillTitle"] as string;
            model.Description = TempData["WikiPrefillDescription"] as string;
            model.ModuleId = TempData["WikiPrefillModuleId"] as int?;
            model.SubModuleId = TempData["WikiPrefillSubModuleId"] as int?;
            model.SourceWikiDocumentId = TempData["WikiPrefillWikiId"] as int?;
        }

        await InitModelAsync(model, projectId);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> ReloadForProject(BacklogModel model)
    {
        ModelState.Clear();

        var projectId = await ResolveProjectIdAsync(model.ProjectId);

        var projectAccess = await GetProjectAccess(projectId);
        if (projectAccess == null || !projectAccess.CanReport && !projectAccess.CanEdit && !projectAccess.CanClose)
            return AccessDenied();

        model.ProjectId = projectId;
        model.ModuleId = null;
        model.SubModuleId = null;
        model.ReleaseId = null;
        model.AssigneeId = null;
        model.TesterId = null;
        model.StatusId = 0;

        await InitModelAsync(model, projectId);

        return View("Create", model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> CreateFromWiki(int wikiId, int projectId, string title, string description, int? moduleId, int? subModuleId)
    {
        TempData["WikiPrefillTitle"] = string.IsNullOrWhiteSpace(title) ? "Untitled requirement" : title.Trim();
        TempData["WikiPrefillDescription"] = description;
        TempData["WikiPrefillModuleId"] = moduleId;
        TempData["WikiPrefillSubModuleId"] = subModuleId;
        TempData["WikiPrefillWikiId"] = wikiId;

        return RedirectToAction("Create", new { project = projectId });
    }

    [HttpPost, FormName("save-continue", "addNew")]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Create(BacklogModel model,
            List<CustomFieldValueModel> fieldValues,
            bool addNew)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var fieldValuesErrors = false;

        if (fieldValues.Any())
        {
            var customFields = await _customFieldService.GetAllMandatoryAsync(model.ProjectId);
            if (customFields.Any())
            {
                var emptyValues = customFields.Where(x => fieldValues.Any(y => y.CustomFieldId == x.Id && string.IsNullOrEmpty(y.Value)));
                fieldValuesErrors = emptyValues.Any();

                foreach (var customField in emptyValues)
                {
                    ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync($"{customField.ResourceKey}.RequiredMsg"));
                }
            }
        }

        if (ModelState.IsValid)
        {
            var entity = _workItemsMapper.ToEntity(model);
            entity.ProjectId = model.ProjectId;
            entity.CreatedById = loggedUser.Id;
            entity.CreatedOn = DateTime.UtcNow;

            if (!entity.DueDate.HasValue && _workItemSettings.DefaultDueInDays > 0)
                entity.DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(_workItemSettings.DefaultDueInDays));

            var documentIds = ReadUploadTokens(model.Token, loggedUser.Id);

            await _backlogItemService.InsertAsync(entity, documentIds);

            if (entity.Id > 0)
            {
                #region Save Custom Field Values

                var entities = fieldValues.Select(x => _workItemsMapper.ToEntity(x)).ToList();
                if (entities.Any())
                {
                    entities.ForEach(f => f.BacklogId = entity.Id);
                    await _backlogItemService.InsertFieldValueAsync(entities);
                }

                #endregion

                var tagIds = await _tagService.ResolveTagIdsAsync(model.SelectedTags);
                await _tagService.SetTaskTagsAsync(entity.Id, tagIds);

                if (entity.AssigneeId != null && entity.AssigneeId > 0)
                {
                    var newEntity = await _backlogItemService.GetByIdAsync(entity.Id);
                    await _notificationChannelService.TaskAssignedAsync(newEntity, false);
                }

                if (entity.TesterId != null && entity.TesterId > 0)
                {
                    var newEntity = await _backlogItemService.GetByIdAsync(entity.Id);
                    await _notificationChannelService.TaskAssignedAsync(newEntity, true);
                }

                return addNew ? RedirectToAction("Create") : RedirectToAction("Index");
            }

            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToCreateTask"));
        }

        if (!fieldValuesErrors)
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Error.UnableToCreateTask"));

        await InitModelAsync(model, model.ProjectId);

        return View(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Edit(int id, int project = 0)
    {
        var projectAccess = await GetProjectAccess(await ResolveProjectIdAsync(project));
        if (projectAccess == null || !projectAccess.CanReport && !projectAccess.CanEdit && !projectAccess.CanClose)
            return AccessDenied();

        var entity = await _backlogItemService.GetByIdAsync(id);
        if (entity == null)
            return RedirectToAction("Index");

        var accessibleProjects = await _userService.GetAllAccessibleProjects(UserId);
        if (!accessibleProjects.Any(x => x.Id == entity.ProjectId))
            return ProjectAccessDenied(string.Format(await _localizationService.GetResourceAsync("Error.ProjectAccessDenied"), entity.Project.Name));

        var model = _workItemsMapper.ToModel(entity);
        model.Owner = entity.CreatedBy.Name;

        await InitModelAsync(model, model.ProjectId);

        return View(model);
    }

    public async Task<IActionResult> Filter(int filterMode = -2, string? filterValue = null, int project = 0, int assignee = 0)
    {
        var model = new BacklogModel();
        await InitFilterModelAsync(model, filterMode, filterValue, project, assignee);

        return PartialView(model);
    }

    #endregion

    #region Actions For Batch Update

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<JsonResponseModel> Delete(int id)
    {
        var entity = await _backlogItemService.GetByIdAsync(id);
        if (entity == null)
            return new JsonResponseModel
            {
                Status = HttpStatusCode.NoData,
                Message = await _localizationService.GetResourceAsync("FormNoData.Description")
            };

        if (!await _backlogItemService.CanDeleteTaskAsync(entity.Id))
            return new JsonResponseModel
            {
                Status = HttpStatusCode.InternalServerError,
                Message = await _localizationService.GetResourceAsync("Backlog.DeleteNotAllowedMsg")
            };

        await _backlogItemService.DeleteAsync(entity);

        return new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.DeleteSuccess")
        };
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Code(int id, bool refresh = false)
    {
        var task = await _backlogItemService.GetByIdAsync(id);
        if (task == null)
            return NoDataPartial();

        var accessible = await _userService.GetAllAccessibleProjects(UserId);
        var viewAll = await _userService.GetAllAccessibleProjects(UserId, fetchOnlyCanViewOthersTask: true);
        var canSee = accessible.Any(x => x.Id == task.ProjectId)
            && (viewAll.Any(x => x.Id == task.ProjectId) || task.AssigneeId == UserId || task.TesterId == UserId);
        if (!canSee)
            return AccessDenied();

        var code = await _taskCodeService.GetTaskCodeAsync(id, refresh);
        var model = new TaskCodeViewModel
        {
            Code = code,
            TaskId = id,
            ProjectId = task.ProjectId,
            CanManageBranch = code.State != TaskCodeState.NotConfigured && code.State != TaskCodeState.NotLinked
                && await _gitBranchService.CanManageBranchAsync(id),
            CanCreateBranch = _gitBranchService.CreationEnabled,
            ClosingMrId = task.MergeRequestId > 0 ? task.MergeRequestId : null
        };

        if (model.ClosingMrId != null)
        {
            var closing = await _gitRepositoryService.GetClosingContextAsync(task);
            model.ClosingMrRepositoryId = closing?.RepositoryId;
            model.ClosingRefPrefix = closing?.Capabilities.RefPrefix ?? "!";
        }

        if (code.State == TaskCodeState.Ok && code.MergeRequests.Count > 0
            && await _permissionService.AuthorizeAsync(PermissionProvider.ReviewMergeRequests))
        {
            var user = await _userService.GetByIdAsync(UserId);
            model.ReviewRepositoryIds = (await _mergeRequestReviewService.GetScopeAsync(user))
                .Where(s => s.ProjectId == task.ProjectId).Select(s => s.RepositoryId).ToList();
            model.CanReview = model.ReviewRepositoryIds.Count > 0;
        }

        return PartialView("_Code", model);
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> BranchContext(string ids, int? repositoryId = null)
    {
        var context = await _gitBranchService.GetContextAsync(ParseIds(ids), repositoryId);
        return Json(new
        {
            status = context.Error == null ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            message = context.Error,
            canCreate = context.CanCreate,
            needsConnection = context.NeedsConnection,
            connectionMessage = context.ConnectionMessage,
            suggestedName = context.SuggestedName,
            prefix = context.Prefix,
            defaultBranch = context.DefaultBranch,
            baseBranches = context.BaseBranches,
            eligible = context.EligibleTaskIds,
            skipped = context.Skipped.Select(x => new { id = x.TaskId, reason = x.Reason }),
            existing = context.Existing.Select(x => new { id = x.TaskId, reason = x.Reason }),
            repositoryId = context.RepositoryId,
            repositories = context.Repositories.Select(x => new { id = x.Id, name = x.Name })
        });
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> MergeRequestContext(int id, int branchId)
    {
        var context = await _gitBranchService.GetMergeRequestContextAsync(id, branchId);
        return Json(new
        {
            status = context.Error == null ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            message = context.Error,
            needsConnection = context.NeedsConnection,
            connectionMessage = context.ConnectionMessage,
            sourceBranch = context.SourceBranch,
            defaultTarget = context.DefaultTarget,
            targetBranches = context.TargetBranches,
            title = context.Title,
            description = context.Description,
            reviewer = context.ReviewerName,
            taskIds = context.TaskIds,
            existingIid = context.ExistingIid,
            existingUrl = context.ExistingUrl,
            openedStatus = context.OpenedStatusName,
            shortTerm = context.ShortTerm,
            term = context.Term,
            refPrefix = context.RefPrefix,
            repositoryName = context.RepositoryName
        });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> CreateMergeRequest(int id, int branchId, string title, string targetBranch, string? description, bool draft = false, bool removeSourceBranch = false)
    {
        var result = await _gitBranchService.CreateMergeRequestAsync(id, branchId, title, targetBranch, description, draft, removeSourceBranch);
        return Json(new
        {
            status = result.Success ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            message = result.Success ? string.Format(_localizationService.GetResource("Backlog.Mr.Created"), $"{result.RefPrefix}{result.Iid}") : result.Error,
            needsConnection = result.NeedsConnection,
            iid = result.Iid,
            url = result.Url,
            moved = result.MovedTasks
        });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> CreateBranch(string ids, string name, string baseBranch, bool includeExisting = false, int? repositoryId = null)
    {
        var result = await _gitBranchService.CreateBranchAsync(ParseIds(ids), name, baseBranch, includeExisting, repositoryId);
        return BranchResultJson(result, "Backlog.Branch.Created");
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> LinkBranch(string ids, string name, int? repositoryId = null)
    {
        var result = await _gitBranchService.LinkBranchAsync(ParseIds(ids), name, repositoryId);
        return BranchResultJson(result, "Backlog.Branch.Linked");
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> UnlinkBranch(int id, int branchId)
    {
        var result = await _gitBranchService.UnlinkBranchAsync(id, branchId);
        return BranchResultJson(result, "Backlog.Branch.Unlinked");
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> SearchBranches(int id, string? term, int? repositoryId = null)
    {
        var (branches, error) = await _gitBranchService.SearchBranchesAsync(id, term, repositoryId);
        var task = await _backlogItemService.GetByIdAsync(id);
        var repositories = task == null ? [] : await _gitRepositoryService.GetProjectContextsAsync(task.ProjectId);
        return Json(new
        {
            status = error == null ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            message = error,
            branches = branches ?? [],
            repositoryId = repositoryId ?? repositories.FirstOrDefault()?.RepositoryId,
            repositories = repositories.Select(r => new { id = r.RepositoryId, name = r.Name })
        });
    }

    private static List<int> ParseIds(string? ids) =>
        (ids ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToInt()).Where(x => x > 0).Distinct().ToList();

    private IActionResult BranchResultJson(BranchActionResult result, string successKey)
    {
        return Json(new
        {
            status = result.Success ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
            message = result.Success ? string.Format(_localizationService.GetResource(successKey), result.BranchName, result.LinkedTaskIds.Count) : result.Error,
            branch = result.BranchName,
            linked = result.LinkedTaskIds,
            needsConnection = result.NeedsConnection
        });
    }

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> BatchUpdate(string ids)
    {
        var taskIds = (ids ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.ToInt())
            .ToArray();

        var projectIds = taskIds.Length > 0
            ? await _backlogItemService.GetProjectIdsByTaskIdsAsync(taskIds)
            : [];

        var model = new BacklogBatchUpdateModel
        {
            TaskIds = ids,
            IsMixedProject = projectIds.Count > 1
        };

        await InitBatchUpdateModelAsync(model, projectIds);

        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> BatchUpdate(BacklogBatchUpdateModel model)
    {
        if (ModelState.IsValid)
        {
            var taskIds = (model.TaskIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToInt())
                .ToArray();

            var projectIds = taskIds.Length > 0
                ? await _backlogItemService.GetProjectIdsByTaskIdsAsync(taskIds)
                : [];

            if (projectIds.Count > 1)
            {
                // Mixed projects: project-bound fields can't be applied.
                model.ModuleId = null;
                model.SubModuleId = null;
                model.AssigneeId = model.AssigneeId == 0 ? 0 : null;
                model.TesterId = model.TesterId == 0 ? 0 : null;

                // Status must be global across projects.
                if (model.StatusId.HasValue && model.StatusId > 0)
                {
                    var status = await _statusService.GetByIdAsync(model.StatusId.Value);
                    if (status != null && status.ProjectId != null)
                        return Json(new JsonResponseModel
                        {
                            Status = HttpStatusCode.ValidationError,
                            Message = "A project-specific status cannot be applied to tasks from multiple projects. Choose a global status."
                        });
                }
            }

            if (model.ReleaseId.HasValue && model.ReleaseId > 0 && projectIds.Count > 0
                && !await _releaseService.IsAvailableForAllProjectsAsync(model.ReleaseId.Value, projectIds))
            {
                return Json(new JsonResponseModel
                {
                    Status = HttpStatusCode.ValidationError,
                    Message = "This release is not mapped to every project of the selected tasks. Choose a release available to all of them."
                });
            }

            await _backlogItemService.UpdateBatchAsync(model.TaskIds, model.TaskTypeId, model.ReporterId, model.SeverityId,
                model.DueDate, model.ModuleId, model.SubModuleId, model.ReleaseId, model.AssigneeId, model.TesterId, model.StatusId);
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

    #endregion

    #region Actions For Documents

    public async Task<IActionResult> Documents(int id)
    {
        var data = await _backlogItemService.GetAllDocumentAsync(id);
        var model = data.Select(s => new BacklogDocumentGridModel
        {
            Id = s.Id,
            Name = s.Document.FileName,
            ContentType = s.Document.ContentType,
            FileSize = s.Document.FileSize,
            Extension = s.Document.Extension
        }).ToList();

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> UploadDocument(IFormFile file, int reference = 0)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No file uploaded." });

        if (!UploadPolicy.IsAllowedAttachment(file.FileName, file.Length))
            return BadRequest(new { error = $"Only {string.Join(", ", UploadPolicy.AttachmentExtensions)} files up to {UploadPolicy.MaxAttachmentMegabytes} MB can be attached." });

        if (reference > 0 && !await _backlogItemService.CanEditTaskAsync(reference))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "You cannot add files to this task." });

        var document = await _documentService.InsertAsync(file);
        if (document == null || document.Id <= 0)
            return BadRequest(new { error = "The file could not be read." });

        if (reference > 0)
        {
            await _backlogItemService.InsertDocumentAsync(reference, document.Id, UserId);
            return Json(new { success = true });
        }

        return Json(new
        {
            success = true,
            token = _uploadTokenService.Create(UserId, document.Id)
        });
    }

    public async Task<IActionResult> ViewDocument(int token)
    {
        var attachment = await _backlogItemService.GetAttachmentAsync(token);
        if (attachment?.Document == null || !await _backlogItemService.CanViewTaskAsync(attachment.BacklogId))
            return NotFound();

        return DocumentFile(attachment.Document);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteDocument(int token)
    {
        var attachment = await _backlogItemService.GetAttachmentAsync(token);
        if (attachment == null || !await _backlogItemService.CanEditTaskAsync(attachment.BacklogId))
            return Json(new { success = false, message = "Oops, unable to delete the file!" });

        var status = await _backlogItemService.DeleteDocumentAsync(token, UserId);

        return Json(new
        {
            success = status,
            message = status ? "File deleted!" : "Oops, unable to delete the file!"
        });
    }

    #endregion

    #region Actions For History

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> History(int id)
    {
        var data = await _backlogItemService.GetAllHistoryAsync(id);
        return PartialView(BacklogHistoryMapper.ToGridModels(data));
    }

    #endregion

    #region Actions For Comments

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Comments(int id)
    {
        var data = await _backlogItemService.GetAllCommentsAsync(id);
        var model = new BacklogCommentModel()
        {
            BackLogId = id,
            CanPostToIssue = await _issueSyncService.HasActiveLinkAsync(id),
            Comments = data.Select(s => new BacklogCommentGridModel
            {
                CommentById = s.CreatedById,
                CommentBy = s.CreatedBy.Name,
                CommentOn = s.CreatedOn,
                Comment = s.Comment,
                SystemComment = s.SystemComment
            }).ToList()
        };

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> CommentCreate(int id, string comment, bool postToIssue = false)
    {
        postToIssue = postToIssue && await _issueSyncService.HasActiveLinkAsync(id);
        await _backlogItemService.InsertCommentAsync(id, comment, UserId, UserName, postToIssue);
        if (postToIssue)
            await _gitJobScheduler.EnqueueIssuePushAsync(id);

        return Json(new
        {
            success = true,
            commentbyid = UserId,
            commentby = UserName,
            commenton = DateTime.Now.ToString("dd-MM-yyyy hh:mm")
        });
    }

    #endregion

    #region Action For Closing Comments

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> ClosingComment(int id, int status)
    {
        var backlogEntity = await _backlogItemService.GetByIdAsync(id);
        if (backlogEntity == null)
            return NotFound();

        var requirement = await _closingRequirementService.EvaluateAsync(id, status, loadMergeRequests: true);
        var model = new ClosingCommentModel
        {
            Id = id,
            Status = status,
            StatusName = requirement.StatusName,
            TaskTitle = backlogEntity.Title,
            RootCauseAnalysis = backlogEntity.RootCauseAnalysis,
            ResolutionDetails = backlogEntity.ResolutionDetails,
            Requirement = requirement
        };

        foreach (var mr in requirement.LinkedMergeRequests)
        {
            model.AvailableMergeRequests.Add(new ClosingMergeRequestOption
            {
                Iid = mr.Iid,
                RepositoryId = mr.RepositoryId,
                Title = mr.Title,
                State = mr.State,
                SourceBranch = mr.SourceBranch,
                Author = mr.Author,
                Linked = true
            });
        }

        var contexts = await _gitRepositoryService.GetProjectContextsAsync(backlogEntity.ProjectId);
        foreach (var option in model.AvailableMergeRequests)
        {
            var context = contexts.FirstOrDefault(c => c.RepositoryId == option.RepositoryId);
            option.RefPrefix = context?.Capabilities.RefPrefix ?? "!";
            option.RepositoryName = contexts.Count > 1 ? context?.Name : null;
        }

        if (requirement.MrMissing)
        {
            var loggedUser = await _workContext.GetCurrentUserAsync();
            foreach (var context in contexts)
            {
                var userName = await _gitAccountService.GetUserNameAsync(loggedUser, context.Connection);
                if (string.IsNullOrEmpty(userName))
                    continue;

                var mergeRequests = await context.Provider.GetOpenMergeRequestsByAuthorAsync(context.RepoRef, userName);
                if (mergeRequests == null)
                    model.GitLabWarning = context.Provider.LastError;
                foreach (var item in (mergeRequests ?? []).Where(m => !model.AvailableMergeRequests.Any(o => o.Iid == m.MrId && o.RepositoryId == context.RepositoryId)))
                {
                    model.AvailableMergeRequests.Add(new ClosingMergeRequestOption
                    {
                        Iid = item.MrId,
                        RepositoryId = context.RepositoryId,
                        RepositoryName = contexts.Count > 1 ? context.Name : null,
                        RefPrefix = context.Capabilities.RefPrefix,
                        Title = item.Title,
                        State = "opened",
                        Author = userName
                    });
                }
            }
        }

        return PartialView(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> ClosingComment(ClosingCommentModel model)
    {
        if (!await ValidateClosingAsync(model))
        {
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("Error.Failed"),
                Errors = ModelState.AllErrors()
            });
        }

        if (!await _backlogItemService.CanEditTaskAsync(model.Id))
        {
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("Backlog.NotAuthorizedEditMsg")
            });
        }

        var backlogEntity = await _backlogItemService.GetByIdAsync(model.Id);
        if (backlogEntity == null)
        {
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.InternalServerError,
                Message = await _localizationService.GetResourceAsync("Error.Failed")
            });
        }

        int? closingRepositoryId = null;
        if (model.MergeRequestRepositoryId > 0)
        {
            var repository = await _gitRepositoryService.GetRepositoryByIdAsync(model.MergeRequestRepositoryId.Value);
            if (repository != null && repository.ProjectId == backlogEntity.ProjectId)
                closingRepositoryId = repository.Id;
        }

        if (model.MergeRequestId > 0 && (backlogEntity.MergeRequestId != model.MergeRequestId || backlogEntity.MergeRequestRepositoryId != closingRepositoryId))
        {
            backlogEntity.MergeRequestId = model.MergeRequestId;
            backlogEntity.MergeRequestRepositoryId = closingRepositoryId;
            await _backlogItemService.UpdateAsync(backlogEntity);
        }

        var changes = new List<FieldChange>();
        if (HtmlText.HasContent(model.RootCauseAnalysis) && model.RootCauseAnalysis != backlogEntity.RootCauseAnalysis)
            changes.Add(new FieldChange { Name = nameof(Taskist.Core.Domain.WorkItems.Backlog.RootCauseAnalysis), Value = model.RootCauseAnalysis });
        if (HtmlText.HasContent(model.ResolutionDetails) && model.ResolutionDetails != backlogEntity.ResolutionDetails)
            changes.Add(new FieldChange { Name = nameof(Taskist.Core.Domain.WorkItems.Backlog.ResolutionDetails), Value = model.ResolutionDetails });
        changes.Add(new FieldChange { Name = nameof(Taskist.Core.Domain.WorkItems.Backlog.StatusId), Value = model.Status.ToString() });

        var message = await _backlogItemService.UpdateFieldsAsync(model.Id, changes);
        if (message.StartsWith("Error"))
        {
            return Json(new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = message,
                Errors = [new ErrorModel(string.Empty, message.Replace("Error: ", string.Empty))]
            });
        }

        if (model.MergeRequestId > 0)
        {
            try
            {
                await _gitBranchService.LinkClosingMergeRequestBranchAsync(model.Id, model.MergeRequestId.Value, closingRepositoryId);
            }
            catch (Exception)
            {
            }
            await _taskCodeService.ClearCacheAsync(model.Id);
        }

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        });
    }

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> ViewClosingComment(int id)
    {
        var backlogEntity = await _backlogItemService.GetByIdAsync(id);
        if (backlogEntity == null || !await CanSeeTaskAsync(backlogEntity))
            return AccessDenied();

        var closing = backlogEntity.MergeRequestId > 0 ? await _gitRepositoryService.GetClosingContextAsync(backlogEntity) : null;
        var model = new ViewClosingCommentModel
        {
            TaskId = backlogEntity.Id,
            ProjectId = backlogEntity.ProjectId,
            RootCauseAnalysis = backlogEntity.RootCauseAnalysis,
            ResolutionDetails = backlogEntity.ResolutionDetails,
            MergeRequestId = backlogEntity.MergeRequestId,
            RefPrefix = closing?.Capabilities.RefPrefix ?? "!"
        };

        return PartialView(model);
    }

    #endregion

    #region Action For Git

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> MergeRequest(int id)
    {
        var model = new MergeRequestDetailsDto();
        var task = await _backlogItemService.GetByIdAsync(id);
        if (task == null || !await CanSeeTaskAsync(task))
            return AccessDenied();

        if (task.MergeRequestId > 0)
        {
            var context = await _gitRepositoryService.GetClosingContextAsync(task);
            if (context != null)
                model = await context.Provider.GetMergeRequestAsync(context.RepoRef, task.MergeRequestId.Value) ?? model;
        }

        return PartialView(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Issue(int id)
    {
        var task = await _backlogItemService.GetByIdAsync(id);
        if (task == null || !await CanSeeTaskAsync(task))
            return AccessDenied();

        var canManage = await _backlogItemService.CanEditTaskAsync(id);
        return PartialView("_Issue", new TaskIssueViewModel
        {
            TaskId = id,
            Issue = await _issueSyncService.GetTaskIssueAsync(id, canManage)
        });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> PublishIssue(int id, int repositoryId)
    {
        if (!await _backlogItemService.CanEditTaskAsync(id))
            return IssueJson(IssueSyncResult.Fail(await _localizationService.GetResourceAsync("Backlog.NotAuthorizedEditMsg")));

        var user = await _workContext.GetCurrentUserAsync();
        return IssueJson(await _issueSyncService.PublishAsync(id, repositoryId, user));
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> SyncIssue(int id)
    {
        if (!await _backlogItemService.CanEditTaskAsync(id))
            return IssueJson(IssueSyncResult.Fail(await _localizationService.GetResourceAsync("Backlog.NotAuthorizedEditMsg")));

        var user = await _workContext.GetCurrentUserAsync();
        return IssueJson(await _issueSyncService.SyncNowAsync(id, user));
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> UnlinkIssue(int id, int linkId)
    {
        if (!await _backlogItemService.CanEditTaskAsync(id))
            return IssueJson(IssueSyncResult.Fail(await _localizationService.GetResourceAsync("Backlog.NotAuthorizedEditMsg")));

        var user = await _workContext.GetCurrentUserAsync();
        return IssueJson(await _issueSyncService.UnlinkAsync(id, linkId, user));
    }

    private IActionResult IssueJson(IssueSyncResult result) => Json(new
    {
        status = result.Success ? HttpStatusCode.Success : HttpStatusCode.ValidationError,
        message = result.Message
    });

    private async Task<bool> CanSeeTaskAsync(Taskist.Core.Domain.WorkItems.Backlog task)
    {
        var accessible = await _userService.GetAllAccessibleProjects(UserId);
        if (accessible.All(x => x.Id != task.ProjectId))
            return false;

        var viewAll = await _userService.GetAllAccessibleProjects(UserId, fetchOnlyCanViewOthersTask: true);
        return viewAll.Any(x => x.Id == task.ProjectId) || task.AssigneeId == UserId || task.TesterId == UserId;
    }

    #endregion

    #region Data

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> DataRead(DataTableRequest request,
            int[] createdby,
            int[] module,
            int[] subModule,
            int[] taskType,
            int[] severity,
            int[] reporter,
            int[] assignee,
            int[] tester,
            int[] status,
            int[] statusGroup,
            int[] release,
            int[] project,
            int? dueDateType,
            DateOnly? dueDate,
            int filterMode = 0,
            bool groupByParent = true,
            int? statusDays = null,
            int? openDays = null)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var model = await GetProjectAccess(await ResolveProjectIdAsync(0));
        var accessibleProjects = await _userService.GetAllAccessibleProjects(loggedUser.Id);
        var accessibleIds = accessibleProjects.Select(x => x.Id).ToArray();
        var viewAllIds = (await _userService.GetAllAccessibleProjects(loggedUser.Id, fetchOnlyCanViewOthersTask: true)).Select(x => x.Id).ToArray();
        var projectScope = (project != null && project.Length > 0)
            ? project.Where(p => accessibleIds.Contains(p)).ToArray()
            : [];

        static bool Any(int[] a) => a != null && a.Length > 0;
        var hasActiveFilter = !string.IsNullOrWhiteSpace(request.SearchValue)
            || filterMode != 0
            || dueDate != null
            || Any(createdby) || Any(module) || Any(subModule) || Any(taskType) || Any(severity)
            || Any(reporter) || Any(assignee) || Any(tester) || Any(status) || Any(statusGroup)
            || Any(release) || Any(project)
            || statusDays > 0 || openDays > 0;

        var data = await _backlogItemService.GetPagedListAsync(loggedUser.Id,
                    0,
                    createdby,
                    module,
                    subModule,
                    taskType,
                    severity,
                    reporter,
                    assignee,
                    tester,
                    status,
                    statusGroup,
                    release,
                    dueDateType,
                    dueDate,
                    accessibleIds,
                    request.SearchValue,
                    request.Start,
                    request.Length,
                    request.SortColumn,
                    request.SortDirection,
                    model.CanViewOthersTask,
                    project: projectScope,
                    viewAllProjects: viewAllIds,
                    filterMode: filterMode,
                    rootsOnly: groupByParent && !hasActiveFilter,
                    statusDaysMin: statusDays,
                    openDaysMin: openDays);

        var childCounts = await _backlogItemService.GetChildCountsAsync(data.Select(x => x.Id).ToArray());
        var statusBounds = await _backlogItemService.GetStatusChangeBoundsAsync(data.Select(x => x.Id).ToArray());
        var branchNames = await _gitBranchService.GetActiveBranchNamesAsync(data.Select(x => x.Id).ToList());

        return Json(new
        {
            request.Draw,
            data = data.Select(x => new BacklogItemGridModel
            {
                Id = x.Id,
                Title = x.Title,
                DueDate = x.DueDate,
                ProjectId = x.ProjectId,
                Project = x.Project.Name,
                ModuleId = x.ModuleId,
                Module = x.Module != null ? x.Module.Name : string.Empty,
                SubModuleId = x.SubModuleId,
                SubModule = x.SubModule != null ? x.SubModule.Name : string.Empty,
                ReleaseId = x.ReleaseId,
                Release = x.Release != null ? x.Release.Name : string.Empty,
                AssigneeId = x.AssigneeId,
                Assignee = x.Assignee != null ? x.Assignee.Name : string.Empty,
                TesterId = x.TesterId,
                Tester = x.Tester != null ? x.Tester.Name : string.Empty,
                ReOpenCount = x.ReOpenCount,
                SubTaskCount = childCounts.TryGetValue(x.Id, out var cc) ? cc : 0,
                CreatedBy = x.CreatedBy.Name,
                CreatedOn = x.CreatedOn,
                StatusSince = statusBounds.TryGetValue(x.Id, out var bounds) ? bounds.Last : null,
                OpenSince = statusBounds.ContainsKey(x.Id) ? statusBounds[x.Id].First : x.CreatedOn,
                AgeDays = AgeInDays(x.CreatedOn, statusBounds.ContainsKey(x.Id) ? statusBounds[x.Id].First : null),
                StatusDays = AgeInDays(x.CreatedOn, statusBounds.ContainsKey(x.Id) ? statusBounds[x.Id].Last : null),
                Reporter = x.Reporter != null ? x.Reporter.Name : string.Empty,
                ModifiedBy = x.ModifiedBy?.Name,
                ModifiedOn = x.ModifiedOn,
                TaskType = _mastersMapper.ToModel(x.TaskType),
                Severity = _mastersMapper.ToModel(x.Severity),
                Status = _mastersMapper.ToModel(x.Status),
                AttachmentCount = x.AttachmentCount == null ? 0 : x.AttachmentCount.ToInt(),
                CommentCount = x.CommentCount == null ? 0 : x.CommentCount.ToInt(),
                MrId = x.MergeRequestId,
                BranchNames = branchNames.TryGetValue(x.Id, out var bn) ? bn : [],
                HasClosingComment = !string.IsNullOrEmpty(x.RootCauseAnalysis)
                    || !string.IsNullOrEmpty(x.ResolutionDetails)
                    || (x.MergeRequestId != null && x.MergeRequestId > 0),
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> Children(int id)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var accessibleIds = (await _userService.GetAllAccessibleProjects(loggedUser.Id)).Select(x => x.Id).ToArray();
        var viewAllIds = (await _userService.GetAllAccessibleProjects(loggedUser.Id, fetchOnlyCanViewOthersTask: true)).Select(x => x.Id).ToArray();

        var children = await _backlogItemService.GetChildrenAsync(id, loggedUser.Id, accessibleIds, viewAllIds);
        var childCounts = await _backlogItemService.GetChildCountsAsync(children.Select(x => x.Id).ToArray());

        return Json(children.Select(x => new BacklogChildRowModel
        {
            Id = x.Id,
            Title = x.Title,
            TaskTypeName = x.TaskType != null ? x.TaskType.Name : string.Empty,
            TaskTypeIcon = x.TaskType != null ? x.TaskType.IconClass : null,
            TaskTypeColor = x.TaskType != null ? x.TaskType.BackgroundColor : null,
            StatusName = x.Status != null ? x.Status.Name : string.Empty,
            StatusColor = x.Status != null ? x.Status.BackgroundColor : null,
            AssigneeName = x.Assignee != null ? x.Assignee.Name : string.Empty,
            DueDate = x.DueDate,
            SubTaskCount = childCounts.TryGetValue(x.Id, out var cc) ? cc : 0
        }));
    }

    #endregion

    #region Ajax

    [HttpPatch]
    public async Task<JsonResponseModel> UpdateStatus(int id, string name, string value)
    {
        if (!await _backlogItemService.CanEditTaskAsync(id))
            return new JsonResponseModel
            {
                Status = HttpStatusCode.ValidationError,
                Message = await _localizationService.GetResourceAsync("Backlog.NotAuthorizedEditMsg")
            };

        var message = await _backlogItemService.UpdateAsync(id, name, value);

        return new JsonResponseModel
        {
            Status = message.StartsWith("Error") ? HttpStatusCode.ValidationError : HttpStatusCode.Success,
            Message = message
        };
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<JsonResponseModel> SaveChanges(int id, List<FieldChange> changes)
    {
        if (changes == null || changes.Count == 0)
            return new JsonResponseModel
            {
                Status = HttpStatusCode.Success,
                Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
            };

        var message = await _backlogItemService.UpdateFieldsAsync(id, changes);
        if (!message.StartsWith("Error") && await _issueSyncService.HasActiveLinkAsync(id))
            await _gitJobScheduler.EnqueueIssuePushAsync(id);

        return new JsonResponseModel
        {
            Status = message.StartsWith("Error") ? HttpStatusCode.InternalServerError : HttpStatusCode.Success,
            Message = message.StartsWith("Error") ? message : await _localizationService.GetResourceAsync("Message.SaveSuccess")
        };
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<JsonResponseModel> SaveTags(int id, string[] tags)
    {
        if (!await _backlogItemService.CanEditTaskAsync(id))
            return new JsonResponseModel
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "You are not allowed to edit this task."
            };

        var tagIds = await _tagService.ResolveTagIdsAsync(tags);
        await _tagService.SetTaskTagsAsync(id, tagIds);

        return new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        };
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<JsonResponseModel> SaveAll(int id, List<FieldChange> changes, string[] tags,
        List<CustomFieldChange> customFields)
    {
        if (!await _backlogItemService.CanEditTaskAsync(id))
            return new JsonResponseModel
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "You are not allowed to edit this task."
            };

        if (changes != null && changes.Count > 0)
        {
            var message = await _backlogItemService.UpdateFieldsAsync(id, changes);
            if (message.StartsWith("Error"))
                return new JsonResponseModel { Status = HttpStatusCode.InternalServerError, Message = message };
        }

        var tagIds = await _tagService.ResolveTagIdsAsync(tags ?? []);
        await _tagService.SetTaskTagsAsync(id, tagIds);

        if (customFields != null)
            foreach (var field in customFields)
                await _backlogItemService.InsertFieldValueAsync(id, field.Reference, field.Value);

        if (await _issueSyncService.HasActiveLinkAsync(id))
            await _gitJobScheduler.EnqueueIssuePushAsync(id);

        return new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        };
    }

    [HttpGet]
    public async Task<IActionResult> GetFilterPresets()
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var presets = await _genericAttributeService.GetAttributeAsync<string>(loggedUser, Constant.BacklogFilterPresets);

        return Content(string.IsNullOrWhiteSpace(presets) ? "{}" : presets, "application/json");
    }

    [HttpPost]
    public async Task<JsonResponseModel> SaveFilterPresets(string presets)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        await _genericAttributeService.SaveAttributeAsync(loggedUser, Constant.BacklogFilterPresets, presets ?? "{}");

        return new JsonResponseModel { Status = HttpStatusCode.Success };
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> ProjectOptions(int project, int statusId = 0)
    {
        var accessible = await _userService.GetAllAccessibleProjects(UserId);
        if (!accessible.Any(x => x.Id == project))
            return Json(new { error = "You do not have access to this project." });

        static object ToOptions(IEnumerable<SelectListItem> items) => items.Select(x => new { text = x.Text, value = x.Value }).ToList();

        var statuses = await GetEditableStatusesAsync(project, UserId);
        var suggestedStatus = statuses.FirstOrDefault(x => x.Id == statusId);
        if (suggestedStatus == null && statusId > 0)
        {
            var current = await _statusService.GetByIdAsync(statusId);
            suggestedStatus = statuses.FirstOrDefault(x => current != null && x.GroupId == current.GroupId)
                ?? statuses.FirstOrDefault(x => x.GroupId == StatusGroup.New.ToInt());
        }

        return Json(new
        {
            releases = ToOptions(await BuildReleaseOptionsAsync(project, null)),
            assignees = ToOptions(await BuildUserOptionsAsync(project, Constant.DeveloperRoleName, UserId, null)),
            testers = ToOptions(await BuildUserOptionsAsync(project, Constant.QualityAssuranceRoleName, UserId, null)),
            statuses = statuses.Select(x => new { text = x.Name, value = x.Id.ToString() }).ToList(),
            suggestedStatusId = suggestedStatus?.Id.ToString()
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetInactiveFilterUsers()
    {
        static object ToOptions(IEnumerable<User> users) => users
            .OrderBy(x => x.Name)
            .Select(x => new { text = $"{x.Name} (Inactive)", value = x.Id.ToString() })
            .ToList();

        var developers = await _userService.GetAllInactiveByRoleAsync(Constant.DeveloperRoleName);
        var testers = await _userService.GetAllInactiveByRoleAsync(Constant.QualityAssuranceRoleName);
        var creators = await _userService.GetAllInactiveAsync();

        return Json(new
        {
            developers = ToOptions(developers),
            testers = ToOptions(testers),
            creators = ToOptions(creators)
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetVisibleColumns()
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var columns = await _genericAttributeService.GetAttributeAsync<string>(loggedUser, Constant.BacklogVisibleColumns);

        return Json(columns ?? string.Empty);
    }

    [HttpPost]
    public async Task<JsonResponseModel> SaveVisibleColumns(string columns)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        await _genericAttributeService.SaveAttributeAsync(loggedUser, Constant.BacklogVisibleColumns, columns ?? string.Empty);

        return new JsonResponseModel { Status = HttpStatusCode.Success };
    }

    [HttpGet]
    public async Task<IActionResult> GetGroupByParent()
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var value = await _genericAttributeService.GetAttributeAsync<string>(loggedUser, Constant.BacklogGroupByParent);

        // Default ON when the user hasn't set a preference.
        var enabled = string.IsNullOrEmpty(value) || value == "true";
        return Json(enabled);
    }

    [HttpPost]
    public async Task<JsonResponseModel> SaveGroupByParent(bool enabled)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        await _genericAttributeService.SaveAttributeAsync(loggedUser, Constant.BacklogGroupByParent, enabled ? "true" : "false");

        return new JsonResponseModel { Status = HttpStatusCode.Success };
    }

    [HttpGet]
    public async Task<List<SelectListItem>> GetModules(int project)
    {
        var modules = await _moduleService.GetAllActiveByProjectAsync(project);

        return modules.OrderBy(x => x.Name)
            .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            .ToList();
    }

    [HttpPatch]
    public async Task<JsonResponseModel> UpdateCustomField(int id, int reference, string value)
    {
        var message = await _backlogItemService.InsertFieldValueAsync(id, reference, value);

        return new JsonResponseModel
        {
            Status = message.StartsWith("Error") ? HttpStatusCode.ValidationError : HttpStatusCode.Success,
            Message = message
        };
    }

    public async Task<ClosingCommentStatusModel> GetClosingCommentStatus(int taskType, int status, int id = 0)
    {
        var response = new ClosingCommentStatusModel();
        if (id <= 0)
            return response;

        var requirement = await _closingRequirementService.EvaluateAsync(id, status, taskType);
        response.Valid = requirement.AnyMissing;
        response.RootCauseMissing = requirement.RootCauseMissing;
        response.ResolutionMissing = requirement.ResolutionMissing;
        response.MrMissing = requirement.MrMissing;

        return response;
    }

    public async Task<bool> CheckStatusCommentsRequired(int status)
    {
        var statusEntity = await _statusService.GetByIdAsync(status);

        return statusEntity != null && (statusEntity.RequiredRootCauseForBug
                        || statusEntity.RequiredRootCauseForCr
                        || statusEntity.RequiredResolutionForBug
                        || statusEntity.RequiredResolutionForCr
                        || statusEntity.RequiredMrNumberForBug
                        || statusEntity.RequiredMrNumberForCr);
    }

    public async Task<JsonResult> GetParentTasks(string q, int id)
    {
        var model = new List<SelectListItem>();
        var entities = await _backlogItemService.SearchByTitleAsync(q, id);

        foreach (var item in entities)
        {
            model.Add(new SelectListItem
            {
                Value = item.Id.ToString(),
                Text = $"{item.Id} - {item.Title}"
            });
        }

        return Json(model);
    }

    public async Task<JsonResult> SimilarTasks(int project, string title, bool all = false)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length < 3)
            return Json(Array.Empty<object>());

        var loggedUser = await _workContext.GetCurrentUserAsync();
        var accessibleProjects = await _userService.GetAllAccessibleProjects(loggedUser.Id);
        var accessibleIds = accessibleProjects.Select(x => x.Id).ToArray();

        var scopeIds = (!all && project > 0 && accessibleIds.Contains(project))
            ? [project]
            : accessibleIds;

        var matches = await _backlogItemService.SearchSimilarAsync(scopeIds, title);
        var trimmed = title.Trim();

        var result = matches.Select(x => new
        {
            id = x.Id,
            title = x.Title,
            project = x.Project?.Name,
            taskType = x.TaskType?.Name,
            status = x.Status?.Name,
            exact = string.Equals(x.Title, trimmed, StringComparison.OrdinalIgnoreCase),
            url = Url.Action("Edit", "Backlog", new { id = x.Id })
        });

        return Json(result);
    }

    #endregion

    #region Exporting

    public async Task<IActionResult> ExportToExcel(int[] createdby,
            int[] module,
            int[] subModule,
            int[] taskType,
            int[] severity,
            int[] reporter,
            int[] assignee,
            int[] tester,
            int[] status,
            int[] statusGroup,
            int[] release,
            int[] project,
            int? dueDateType,
            DateOnly? dueDate,
            string groups,
            int? statusDays = null,
            int? openDays = null)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var model = await GetProjectAccess(await ResolveProjectIdAsync(0));
        var accessibleProjects = await _userService.GetAllAccessibleProjects(loggedUser.Id);
        var accessibleIds = accessibleProjects.Select(x => x.Id).ToArray();
        var viewAllIds = (await _userService.GetAllAccessibleProjects(loggedUser.Id, fetchOnlyCanViewOthersTask: true)).Select(x => x.Id).ToArray();
        var projectScope = (project != null && project.Length > 0)
            ? project.Where(p => accessibleIds.Contains(p)).ToArray()
            : [];

        var groupByColumns = string.IsNullOrWhiteSpace(groups) ? [] : groups.Split(',');

        var entities = await _backlogItemService.GetAllForExcelExportAsync(loggedUser.Id,
                    0,
                    createdby,
                    module,
                    subModule,
                    taskType,
                    severity,
                    reporter,
                    assignee,
                    tester,
                    status,
                    statusGroup,
                    release,
                    dueDateType,
                    dueDate,
                    accessibleIds,
                    groupByColumns,
                    model.CanViewOthersTask,
                    project: projectScope,
                    viewAllProjects: viewAllIds,
                    statusDaysMin: statusDays,
                    openDaysMin: openDays);

        var entityList = entities.ToList();
        var taskIds = entityList.Select(x => x.Id).ToArray();
        var statusBounds = await _backlogItemService.GetStatusChangeBoundsAsync(taskIds);
        var tagNames = await _tagService.GetTagNamesByTaskIdsAsync(taskIds);

        var data = entityList.Select(x => new BacklogExportModel
        {
            Id = x.Id,
            Title = x.Title,
            Description = WebHelper.StripHtmlTags(x.Description),
            RootCauseAnalysis = WebHelper.StripHtmlTags(x.RootCauseAnalysis),
            ResolutionDetails = WebHelper.StripHtmlTags(x.ResolutionDetails),
            DueDate = x.DueDate != null ? "T" : "",
            Project = x.Project.Name,
            Module = x.Module != null ? x.Module.Name : "-",
            SubModule = x.SubModule != null ? x.SubModule.Name : "-",
            Release = x.Release?.Name,
            Assignee = x.Assignee != null ? x.Assignee.Name : "Unassigned",
            ReOpenCount = x.ReOpenCount,
            SubTaskCount = x.SubTaskCount,
            CreatedBy = x.CreatedBy.Name,
            CreatedOn = $"{x.CreatedOn:dd-MMM-yyyy} {x.CreatedOn.ToAge()}",
            AgeDays = IsOpenStatusGroup(x.Status.GroupId)
                ? AgeInDays(x.CreatedOn, statusBounds.ContainsKey(x.Id) ? statusBounds[x.Id].First : null)
                : null,
            TaskType = x.TaskType.Name,
            Severity = x.Severity.Name,
            Status = x.Status.Name,
            Tags = tagNames.GetValueOrDefault(x.Id)
        });

        var exportColumns = await _reportLayoutService.GetExportLayoutAsync(ReportExportKey.Backlogs);
        var bytes = ReportExcelBuilder.Build(data, exportColumns, "Tasks", groupByColumns);

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Backlog Tasks.xlsx");
    }

    public async Task<IActionResult> ExportReleaseToExcel(int releaseId)
    {
        var release = await _releaseService.GetByIdAsync(releaseId);
        var entities = (await _backlogItemService.GetAllByReleaseAsync(releaseId)).ToList();

        var taskIds = entities.Select(x => x.Id).ToArray();
        var tagNames = await _tagService.GetTagNamesByTaskIdsAsync(taskIds);

        var data = entities.Select(x => new BacklogExportModel
        {
            Id = x.Id,
            Title = x.Title,
            Description = WebHelper.StripHtmlTags(x.Description),
            RootCauseAnalysis = WebHelper.StripHtmlTags(x.RootCauseAnalysis),
            ResolutionDetails = WebHelper.StripHtmlTags(x.ResolutionDetails),
            DueDate = x.DueDate != null ? x.DueDate.ToDateString("dd-MMM-yyyy") : "",
            Project = x.Project.Name,
            Module = x.Module != null ? x.Module.Name : "-",
            SubModule = x.SubModule != null ? x.SubModule.Name : "-",
            Release = x.Release?.Name,
            Assignee = x.Assignee != null ? x.Assignee.Name : "Unassigned",
            Tester = x.Tester != null ? x.Tester.Name : "Unassigned",
            ReOpenCount = x.ReOpenCount,
            SubTaskCount = x.SubTaskCount,
            CreatedBy = x.CreatedBy.Name,
            CreatedOn = $"{x.CreatedOn:dd-MMM-yyyy}",
            TaskType = x.TaskType.Name,
            Severity = x.Severity.Name,
            Status = x.Status.Name,
            Tags = tagNames.GetValueOrDefault(x.Id)
        });

        var columns = await _reportLayoutService.GetExportLayoutAsync(ReportExportKey.ReleaseTasks);
        var bytes = ReportExcelBuilder.Build(data, columns, "Tasks");

        var fileName = string.IsNullOrWhiteSpace(release?.Name) ? "Release Tasks" : release.Name.Trim();
        fileName = string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
    }

    #endregion

    #region Helper

    private List<int> ReadUploadTokens(string? json, int userId)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        List<string>? tokens;
        try
        {
            tokens = JsonConvert.DeserializeObject<List<string>>(json);
        }
        catch (JsonException)
        {
            return [];
        }

        return (tokens ?? [])
            .Select(token => _uploadTokenService.Read(token, userId))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
    }

    private async Task<bool> ValidateClosingAsync(ClosingCommentModel model)
    {
        if (!ModelState.IsValid)
            return false;

        var requirement = await _closingRequirementService.EvaluateAsync(model.Id, model.Status);

        if (requirement.RootCauseMissing && !HtmlText.HasContent(model.RootCauseAnalysis))
            ModelState.AddModelError(nameof(ClosingCommentModel.RootCauseAnalysis),
                await _localizationService.GetResourceAsync("ClosingCommentModel.RootCauseAnalysis.RequiredMsg"));

        if (requirement.ResolutionMissing && !HtmlText.HasContent(model.ResolutionDetails))
            ModelState.AddModelError(nameof(ClosingCommentModel.ResolutionDetails),
                await _localizationService.GetResourceAsync("ClosingCommentModel.ResolutionDetails.RequiredMsg"));

        if (requirement.MrMissing && !(model.MergeRequestId > 0))
            ModelState.AddModelError(nameof(ClosingCommentModel.MergeRequestId),
                await _localizationService.GetResourceAsync("ClosingCommentModel.MergeRequestId.RequiredMsg"));

        return ModelState.IsValid;
    }

    private async Task<List<SelectListItem>> BuildUserOptionsAsync(int projectId, string roleName, int loggedUserId, int? selectedId)
    {
        var users = await _userService.GetAllActiveByProjectAsync(projectId, roleName);
        var items = new List<SelectListItem>();

        if (users.Any(x => x.Id == loggedUserId))
            items.Add(new SelectListItem { Text = "Myself", Value = loggedUserId.ToString(), Selected = loggedUserId == selectedId });

        foreach (var item in users.Where(x => x.Id != loggedUserId).OrderBy(x => x.Name))
            items.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString(), Selected = item.Id == selectedId });

        return items;
    }

    private async Task<List<SelectListItem>> BuildReleaseOptionsAsync(int projectId, int? selectedId)
    {
        return (await _releaseService.GetAllActiveByProjectAsync(projectId))
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == selectedId })
            .ToList();
    }

    private async Task<List<Status>> GetEditableStatusesAsync(int projectId, int userId)
    {
        var statuses = await _statusService.GetAllActiveByProjectAsync(projectId);
        var mapping = await _projectService.GetMappingForUserAsync(projectId, userId);

        if (mapping == null || !mapping.CanReOpen)
            statuses = statuses.Where(x => x.GroupId != StatusGroup.ReOpened.ToInt()).ToList();
        if (mapping == null || !mapping.CanClose)
            statuses = statuses.Where(x => x.GroupId != StatusGroup.Resolved.ToInt()).ToList();

        return statuses.OrderBy(x => x.Name).ToList();
    }

    private static int AgeInDays(DateTime createdOnUtc, DateTime? statusLoggedOn)
    {
        var days = statusLoggedOn.HasValue
            ? (DateTime.Now - statusLoggedOn.Value).Days
            : (DateTime.UtcNow - createdOnUtc).Days;

        return Math.Max(0, days);
    }

    private static bool IsOpenStatusGroup(int groupId)
    {
        return groupId == StatusGroup.New.ToInt()
            || groupId == StatusGroup.InProgress.ToInt()
            || groupId == StatusGroup.ReOpened.ToInt();
    }

    private async Task<int> ResolveProjectIdAsync(int projectId)
    {
        var accessible = await _userService.GetAllAccessibleProjects(UserId);
        if (projectId > 0 && accessible.Any(p => p.Id == projectId))
            return projectId;
        return accessible.FirstOrDefault()?.Id ?? 0;
    }

    private async Task<BacklogGridModel> GetProjectAccess(int projectId)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var projectMap = await _userService.GetProjectMapping(loggedUser.Id, projectId);

        return new BacklogGridModel
        {
            ProjectId = projectMap != null ? projectMap.Id : 0,
            CanReport = projectMap != null && (projectMap.CanReportToDo || projectMap.CanReportCR || projectMap.CanReportBug),
            CanReportToDo = projectMap != null && projectMap.CanReportToDo,
            CanReportCR = projectMap != null && projectMap.CanReportCR,
            CanReportBug = projectMap != null && projectMap.CanReportBug,
            CanEdit = projectMap != null && projectMap.CanEdit,
            CanClose = projectMap != null && projectMap.CanClose,
            CanViewOthersTask = projectMap != null && projectMap.CanViewOthersTask,
            CanEditOthersTask = projectMap != null && projectMap.CanEditOthersTask
        };
    }

    private async Task InitModelAsync(BacklogModel model, int projectId)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var projectAccess = await GetProjectAccess(projectId);

        var taskTypes = await _taskTypeService.GetAllActiveAsync();
        var reporters = await _reporterService.GetAllActiveAsync();
        var severities = await _severityService.GetAllActiveAsync();
        var modules = await _moduleService.GetAllActiveByProjectAsync(projectId);
        var status = await _statusService.GetAllActiveByProjectAsync(projectId);

        if (model.Id > 0)
        {
            var taskMapping = await _projectService.GetMappingForUserAsync(model.ProjectId, loggedUser.Id);
            model.CanEdit = model.AssigneeId == loggedUser.Id || (taskMapping != null && taskMapping.CanEditOthersTask);
            if (model.ParentId > 0)
            {
                var parent = await _backlogItemService.GetByIdAsync(model.ParentId.ToInt());
                if (parent != null)
                    model.ParentTitle = $"{model.ParentId} - {parent.Title}";
            }

            #region Status Log

            var statusLogEntities = await _backlogItemService.GetStatusLogByTaskId(model.Id);
            for (int i = 0; i < statusLogEntities.Count; i++)
            {
                var current = statusLogEntities[i];
                var next = i + 1 < statusLogEntities.Count ? statusLogEntities[i + 1] : null;

                var isTerminal = current.Status.GroupId == (int)StatusGroup.Resolved
                    || current.Status.GroupId == (int)StatusGroup.Archived;

                var start = current.CreatedOn;
                var end = next?.CreatedOn ?? (isTerminal ? start : DateTime.Now);

                model.StatusLogs.Add(new StatusLogModel
                {
                    Status = _mastersMapper.ToModel(current.Status),
                    DaysHeld = (end - start).Days,
                    StartDate = start,
                    EndDate = end
                });
            }

            #endregion

            #region Stage duration chips

            var stageMetrics = await _stageMetricService.GetActiveForProjectAsync(model.ProjectId, onlyShowOnTask: true);
            if (stageMetrics.Count > 0 && statusLogEntities.Count > 0)
            {
                var orderedLog = statusLogEntities
                    .OrderBy(x => x.CreatedOn)
                    .Select(x => (x.StatusId, x.CreatedOn))
                    .ToList();

                foreach (var metric in stageMetrics)
                {
                    var span = StageDurationCalculator.FirstPassSpan(orderedLog, metric.FromStatusId, metric.ToStatusId);
                    if (span == null)
                        continue;

                    model.StageDurations.Add(new StageDurationChipModel
                    {
                        Name = metric.Name,
                        FromStatus = metric.FromStatus?.Name,
                        ToStatus = metric.ToStatus?.Name,
                        From = span.Value.From,
                        To = span.Value.To,
                        DurationText = StageDurationCalculator.SmartText((span.Value.To - span.Value.From).TotalDays)
                    });
                }
            }

            #endregion
        }

        foreach (var item in taskTypes)
        {
            if (!projectAccess.CanReportToDo && item.GroupId == TaskTypeGroup.ToDo.ToInt()) continue;
            if (!projectAccess.CanReportCR && item.GroupId == TaskTypeGroup.CR.ToInt()) continue;
            if (!projectAccess.CanReportBug && item.GroupId == TaskTypeGroup.Bug.ToInt()) continue;

            model.AvailableTaskTypes.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.TaskTypeId
            });
        }

        var bugTypeIds = taskTypes.Where(x => x.GroupId == TaskTypeGroup.Bug.ToInt()).Select(x => x.Id).ToList();
        model.BugTaskTypeIds = string.Join(",", bugTypeIds);
        model.IsBug = bugTypeIds.Contains(model.TaskTypeId);
        var crTypeIds = taskTypes.Where(x => x.GroupId == TaskTypeGroup.CR.ToInt()).Select(x => x.Id).ToList();
        model.CrTaskTypeIds = string.Join(",", crTypeIds);
        model.ShowRootCauseTab = model.IsBug || crTypeIds.Contains(model.TaskTypeId);
        model.ClosingRulesJson = JsonConvert.SerializeObject(status
            .Where(x => x.RequiredRootCauseForBug || x.RequiredRootCauseForCr || x.RequiredResolutionForBug
                || x.RequiredResolutionForCr || x.RequiredMrNumberForBug || x.RequiredMrNumberForCr)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name,
                bug = new { rc = x.RequiredRootCauseForBug, res = x.RequiredResolutionForBug, mr = x.RequiredMrNumberForBug },
                cr = new { rc = x.RequiredRootCauseForCr, res = x.RequiredResolutionForCr, mr = x.RequiredMrNumberForCr }
            }));

        foreach (var item in reporters.OrderBy(x => x.Name))
        {
            model.AvailableReporters.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ReporterId
            });
        }

        foreach (var item in severities.OrderBy(x => x.Name))
        {
            model.AvailableSeverities.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.SeverityId
            });
        }

        foreach (var item in modules.OrderBy(x => x.Name))
        {
            model.AvailableModules.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.ModuleId
            });
        }

        if (model.ModuleId > 0)
        {
            var subModules = await _subModuleService.GetAllActiveByModuleAsync(model.ModuleId.ToInt());
            foreach (var item in subModules.OrderBy(x => x.Name))
            {
                model.AvailableSubModules.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                    Selected = item.Id == model.SubModuleId
                });
            }
        }

        foreach (var item in await BuildUserOptionsAsync(projectId, Constant.DeveloperRoleName, loggedUser.Id, model.AssigneeId))
            model.AvailableAssignees.Add(item);

        foreach (var item in await BuildUserOptionsAsync(projectId, Constant.QualityAssuranceRoleName, loggedUser.Id, model.TesterId))
            model.AvailableTesters.Add(item);

        foreach (var item in await BuildReleaseOptionsAsync(projectId, model.ReleaseId))
            model.AvailableReleases.Add(item);

        if (model.ReleaseId > 0 && !model.AvailableReleases.Any(x => x.Value == model.ReleaseId.ToString()))
        {
            var currentRelease = await _releaseService.GetByIdAsync(model.ReleaseId.Value);
            if (currentRelease != null)
                model.AvailableReleases.Add(new SelectListItem
                {
                    Text = currentRelease.Name,
                    Value = currentRelease.Id.ToString(),
                    Selected = true
                });
        }

        if (model.Id == 0 && model.ProjectId == 0)
            model.ProjectId = projectId;

        foreach (var project in (await _userService.GetAllAccessibleProjects(loggedUser.Id)).OrderBy(x => x.Name))
        {
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = project.Name,
                Value = project.Id.ToString(),
                Selected = project.Id == model.ProjectId
            });
        }

        var formProject = await _projectService.GetByIdAsync(model.ProjectId);
        var isPmOrTeamLead = formProject != null &&
            (formProject.ProjectManagerId == loggedUser.Id || formProject.TeamLeadId == loggedUser.Id);
        var projectMap = await _userService.GetProjectMapping(loggedUser.Id, model.ProjectId);
        var canChangeProject = projectMap != null && projectMap.CanChangeProject;
        model.CanEditProject = model.Id == 0 || isPmOrTeamLead || canChangeProject;
        model.CanDelete = model.Id > 0 && await _backlogItemService.CanDeleteTaskAsync(model.Id);

        foreach (var tag in await _tagService.GetAllActiveAsync())
            model.AvailableTags.Add(new SelectListItem { Text = tag.Name, Value = tag.Id.ToString() });

        if (model.Id > 0)
            model.SelectedTags = (await _tagService.GetTagIdsByTaskAsync(model.Id)).Select(x => x.ToString()).ToArray();

        var newStatus = status.Where(x => x.GroupId == (int)StatusGroup.New)
            .OrderByDescending(x => x.ProjectId.HasValue)
            .FirstOrDefault();

        if (model.Id == 0)
        {
            if (newStatus != null)
            {
                model.AvailableStatus.Add(new SelectListItem
                {
                    Text = newStatus.Name,
                    Value = newStatus.Id.ToString(),
                    Selected = true
                });
            }
        }
        else
        {
            foreach (var item in await GetEditableStatusesAsync(projectId, loggedUser.Id))
            {
                model.AvailableStatus.Add(new SelectListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString(),
                    Selected = item.Id == model.StatusId
                });
            }

            if (model.StatusId > 0 && !model.AvailableStatus.Any(x => x.Value == model.StatusId.ToString()))
            {
                var currentStatus = await _statusService.GetByIdAsync(model.StatusId);
                if (currentStatus != null)
                    model.AvailableStatus.Add(new SelectListItem
                    {
                        Text = currentStatus.Name,
                        Value = currentStatus.Id.ToString(),
                        Selected = true
                    });
            }
        }
    }

    private async Task InitBatchUpdateModelAsync(BacklogBatchUpdateModel model, List<int> projectIds)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var isMixed = projectIds.Count > 1;
        var singleProjectId = projectIds.Count == 1 ? projectIds[0] : 0;

        model.AvailableAssignees.Add(new SelectListItem { Text = "Unassigned", Value = "0" });
        model.AvailableTesters.Add(new SelectListItem { Text = "Unassigned", Value = "0" });

        // Global fields are always available (valid across any project).
        foreach (var item in await _taskTypeService.GetAllActiveAsync())
            model.AvailableTaskTypes.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

        foreach (var item in await _reporterService.GetAllActiveAsync())
            model.AvailableReporters.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

        foreach (var item in await _severityService.GetAllActiveAsync())
            model.AvailableSeverities.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

        if (!isMixed && singleProjectId > 0)
        {
            // Single project: scope the project-bound fields to that project.
            var modules = await _moduleService.GetAllActiveByProjectAsync(singleProjectId);
            var assignees = await _userService.GetAllActiveByProjectAsync(singleProjectId, Constant.DeveloperRoleName);
            var testers = await _userService.GetAllActiveByProjectAsync(singleProjectId, Constant.QualityAssuranceRoleName);
            var releases = await _releaseService.GetAllActiveByProjectAsync(singleProjectId);
            var status = await _statusService.GetAllActiveByProjectAsync(singleProjectId);

            foreach (var item in modules)
                model.AvailableModules.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

            var mySelf = assignees.FirstOrDefault(x => x.Id == loggedUser.Id);
            if (mySelf != null)
            {
                assignees = assignees.Where(x => x.Id != mySelf.Id).ToList();
                model.AvailableAssignees.Add(new SelectListItem { Text = "Myself", Value = mySelf.Id.ToString() });
            }
            foreach (var item in assignees)
                model.AvailableAssignees.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

            var mySelfTester = testers.FirstOrDefault(x => x.Id == loggedUser.Id);
            if (mySelfTester != null)
            {
                testers = testers.Where(x => x.Id != mySelfTester.Id).ToList();
                model.AvailableTesters.Add(new SelectListItem { Text = "Myself", Value = mySelfTester.Id.ToString() });
            }
            foreach (var item in testers)
                model.AvailableTesters.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

            model.AvailableReleases.Add(new SelectListItem { Text = "No Release", Value = "0" });
            foreach (var item in releases)
                model.AvailableReleases.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

            var mapping = await _projectService.GetMappingForUserAsync(singleProjectId, loggedUser.Id);
            var filteredStatus = status;
            if (mapping == null || !mapping.CanReOpen)
                filteredStatus = filteredStatus.Where(x => x.GroupId != StatusGroup.ReOpened.ToInt()).ToList();
            if (mapping == null || !mapping.CanClose)
                filteredStatus = filteredStatus.Where(x => x.GroupId != StatusGroup.Resolved.ToInt()).ToList();

            foreach (var item in filteredStatus)
                model.AvailableStatus.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
        }
        else
        {
            // Mixed projects: project-bound fields (Module/SubModule) are disabled
            // in the view. Only global statuses, and releases available to every selected project, apply.
            var commonStatus = (await _statusService.GetAllActiveAsync()).Where(x => x.ProjectId == null);
            foreach (var item in commonStatus)
                model.AvailableStatus.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });

            model.AvailableReleases.Add(new SelectListItem { Text = "No Release", Value = "0" });
            foreach (var item in await _releaseService.GetAllActiveForAllProjectsAsync(projectIds))
                model.AvailableReleases.Add(new SelectListItem { Text = item.Name, Value = item.Id.ToString() });
        }
    }

    private async Task InitFilterModelAsync(BacklogModel model, int filterMode, string? filterValue = null, int filterProjectId = 0, int assigneeId = 0)
    {
        model.AssigneeId = assigneeId > 0 ? assigneeId : null;
        if (filterProjectId > 0)
            model.ProjectId = filterProjectId;
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var accessibleProjects = await _userService.GetAllAccessibleProjects(loggedUser.Id);
        var accessibleProjectIds = accessibleProjects.Select(x => x.Id).ToList();

        var taskTypes = await _taskTypeService.GetAllActiveAsync();
        var reporters = await _reporterService.GetAllActiveAsync();
        var severities = await _severityService.GetAllActiveAsync();
        var modules = await _moduleService.GetAllActiveByProjectAsync(accessibleProjectIds);
        var subModules = await _subModuleService.GetAllActiveAsync();
        var assignees = await _userService.GetAllActiveByRoleAsync(Constant.DeveloperRoleName);
        var testers = await _userService.GetAllActiveByRoleAsync(Constant.QualityAssuranceRoleName);
        var status = await _statusService.GetAllActiveAsync();
        var releases = await _releaseService.GetAllForAnyProjectAsync(accessibleProjectIds);

        model.AvailableTaskTypes = [];

        model.AvailableSeverities = [];
        model.AvailableModules = [];
        model.AvailableSubModules = [];

        model.AvailableStatus = [];
        model.AvailableStatusGroup = [];
        model.AvailableProjects = [];

        foreach (var item in accessibleProjects.OrderBy(x => x.Name))
        {
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = filterProjectId > 0 && item.Id == filterProjectId
            });
        }

        foreach (var item in taskTypes)
        {
            model.AvailableTaskTypes.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        foreach (var item in reporters)
        {
            model.AvailableReporters.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        var reporterPlaceholder = model.AvailableReporters.FirstOrDefault(x => x.Value == "-1" || x.Value == "");
        if (reporterPlaceholder != null)
            reporterPlaceholder.Value = "0";

        foreach (var item in severities)
        {
            model.AvailableSeverities.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        foreach (var item in modules)
        {
            model.AvailableModules.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        foreach (var item in subModules)
        {
            model.AvailableSubModules.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        var mySelf = assignees.FirstOrDefault(x => x.Id == loggedUser.Id);
        if (mySelf != null)
        {
            assignees = assignees.Where(x => x.Id != mySelf.Id).ToList();
            model.AvailableAssignees.Add(new SelectListItem
            {
                Text = "Myself",
                Value = mySelf.Id.ToString(),
                Selected = mySelf.Id == assigneeId
            });
        }

        foreach (var item in assignees)
        {
            model.AvailableAssignees.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.AssigneeId
            });
        }

        var allCreators = (await _userService.GetAllActiveAsync()).OrderBy(x => x.Name).ToList();

        if (allCreators.Any(x => x.Id == loggedUser.Id))
        {
            model.AvailableCreators.Add(new SelectListItem
            {
                Text = "Myself",
                Value = loggedUser.Id.ToString(),
                Selected = loggedUser.Id == model.CreatedById
            });
        }

        foreach (var item in allCreators.Where(x => x.Id != loggedUser.Id))
        {
            model.AvailableCreators.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.CreatedById
            });
        }

        var mySelfTester = testers.FirstOrDefault(x => x.Id == loggedUser.Id);
        if (mySelfTester != null)
        {
            testers = testers.Where(x => x.Id != mySelfTester.Id).ToList();
            model.AvailableTesters.Add(new SelectListItem
            {
                Text = "Myself",
                Value = mySelfTester.Id.ToString()
            });
        }

        model.AvailableAssignees.First(x => x.Value == "-1").Value = "0";

        foreach (var item in testers)
        {
            model.AvailableTesters.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString(),
                Selected = item.Id == model.TesterId
            });
        }

        model.AvailableTesters.First(x => x.Value == "-1").Value = "0";

        var hasReleaseId = int.TryParse(filterValue, out var releaseFilterId);
        foreach (var item in releases)
        {
            model.AvailableReleases.Add(new SelectListItem
            {
                Text = $"{item.Name} - {item.Status.ToString<ReleaseStatus>()}",
                Value = item.Id.ToString(),
                Selected = filterMode == 4 && (hasReleaseId ? item.Id == releaseFilterId : item.Name == filterValue)
            });
        }

        model.AvailableReleases.First(x => x.Value == "-1").Value = "0";

        foreach (var item in status)
        {
            model.AvailableStatus.Add(new SelectListItem
            {
                Text = item.Name,
                Value = item.Id.ToString()
            });
        }

        model.AvailableStatusGroup = Enum.GetValues(typeof(StatusGroup))
                    .Cast<StatusGroup>()
                    .Select(e => new SelectListItem
                    {
                        Value = Convert.ToInt32(e).ToString(),
                        Text = e.ToString(),
                        Selected = (filterMode == 0 && e == StatusGroup.InProgress)
                            || (assigneeId > 0 && (e == StatusGroup.New || e == StatusGroup.InProgress || e == StatusGroup.ReOpened))
                    }).ToList();
    }

    #endregion
}
