using Microsoft.EntityFrameworkCore;
using System.Buffers;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Taskist.Core.Common;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Charts;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.Notification;
using Taskist.Core.Dtos.Reports;
using Taskist.Core.Dtos.WorkItems;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Data.Extensions;
using Taskist.Data.Repository;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Users;

namespace Taskist.Service.WorkItems;

public class BacklogService : IBacklogService
{
    #region Fields

    protected readonly IRepository<Backlog> _backlogRepository;
    protected readonly IRepository<BacklogComment> _backlogCommentRepository;
    protected readonly IRepository<BacklogDocument> _backlogDocumentRepository;
    protected readonly IRepository<BacklogStatusLog> _backlogStatusLogRepository;
    protected readonly IRepository<BacklogCustomFieldValue> _backlogCustomFieldValueRepository;
    protected readonly ITaskTypeService _taskTypeService;
    protected readonly IReporterService _repoterService;
    protected readonly ISeverityService _severityService;
    protected readonly IModuleService _moduleService;
    protected readonly ISubModuleService _subModuleService;
    protected readonly IReleaseService _releaseService;
    protected readonly IUserService _userService;
    protected readonly IStatusService _statusService;
    protected readonly ICustomFieldService _customFieldService;
    protected readonly IMessageService _messageService;
    protected readonly INotificationChannelService _notificationChannelService;
    protected readonly IWorkContext _workContext;
    protected readonly WorkItemSettings _workItemSettings;
    protected readonly IClosingRequirementService _closingRequirementService;
    protected readonly IntegrationSettings _integrationSettings;

    #endregion

    #region Ctor

    public BacklogService(IRepository<Backlog> backlogRepository,
        IRepository<BacklogDocument> backlogDocumentRepository,
        IRepository<BacklogComment> backlogCommentRepository,
        IRepository<BacklogStatusLog> backlogStatusLogRepository,
        IRepository<BacklogCustomFieldValue> backlogCustomFieldValueRepository,
        ITaskTypeService taskTypeService,
        IReporterService reporterService,
        ISeverityService severityService,
        IModuleService moduleService,
        ISubModuleService subModuleService,
        IReleaseService releaseService,
        IUserService userService,
        IStatusService statusService,
        ICustomFieldService customFieldService,
        IMessageService messageService,
        INotificationChannelService notificationChannelService,
        IWorkContext workContext,
        WorkItemSettings workItemSettings,
        IClosingRequirementService closingRequirementService,
        IntegrationSettings integrationSettings)
    {
        _integrationSettings = integrationSettings;
        _closingRequirementService = closingRequirementService;
        _backlogRepository = backlogRepository;
        _backlogDocumentRepository = backlogDocumentRepository;
        _backlogCommentRepository = backlogCommentRepository;
        _backlogStatusLogRepository = backlogStatusLogRepository;
        _backlogCustomFieldValueRepository = backlogCustomFieldValueRepository;
        _taskTypeService = taskTypeService;
        _repoterService = reporterService;
        _severityService = severityService;
        _moduleService = moduleService;
        _subModuleService = subModuleService;
        _releaseService = releaseService;
        _userService = userService;
        _statusService = statusService;
        _customFieldService = customFieldService;
        _messageService = messageService;
        _notificationChannelService = notificationChannelService;
        _workContext = workContext;
        _workItemSettings = workItemSettings;
    }

    #endregion

    #region Methods

    public async Task<IPagedList<Backlog>> GetPagedListAsync(int assigneeId,
        int projectId,
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
        int? dueDateType,
        DateOnly? dueDate,
        int[] accessiableProjects,
        string? search = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        string sortColumn = "",
        string sortDirection = "",
        bool canViewOthersTask = false,
        TaskSource? source = null,
        int[] project = null,
        int[] viewAllProjects = null,
        int filterMode = 0,
        bool rootsOnly = false,
        int? statusDaysMin = null,
        int? openDaysMin = null)
    {
        return await _backlogRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted);

            // Eager-load grid nav columns so the projection doesn't lazy-load them per row.
            query = query.Include(x => x.Reporter);

            if (rootsOnly)
                query = query.Where(x => x.ParentId == null || x.Parent.Deleted);

            if (source != null)
                query = query.Where(x => x.SourceId == (int)source);

            if (filterMode == 1)
                query = query.Where(x => x.AssigneeId == assigneeId || x.TesterId == assigneeId);
            else if (filterMode == 2)
                query = query.Where(x => x.AssigneeId == assigneeId && x.ReOpenCount > 0);
            else if (filterMode == 3)
                query = query.Where(x => x.AssigneeId == assigneeId && x.Status.GroupId == (int)StatusGroup.Resolved);
            else if (filterMode == 6)
            {
                var overdueBefore = DateOnly.FromDateTime(DateTime.Today);
                query = query.Where(x => x.AssigneeId == assigneeId && x.DueDate != null && x.DueDate < overdueBefore);
            }

            if (project != null && project.Length > 0)
                query = query.Where(x => project.Contains(x.ProjectId));
            else if (projectId > 0)
                query = query.Where(x => x.ProjectId == projectId);
            else
                query = query.Where(x => accessiableProjects.Contains(x.ProjectId));

            if (viewAllProjects != null)
                query = query.Where(x => viewAllProjects.Contains(x.ProjectId) || x.AssigneeId == assigneeId || x.TesterId == assigneeId);
            else if (projectId > 0 && !canViewOthersTask)
                query = query.Where(x => x.AssigneeId == assigneeId || x.TesterId == assigneeId);

            if (!string.IsNullOrEmpty(search))
            {
                if (search.StartsWith("#"))
                {
                    var idPart = search.Substring(1);
                    if (int.TryParse(idPart, out int id))
                    {
                        query = query.Where(x => x.Id == id);
                    }
                }
                else
                {
                    query = query.Where(c => c.Title.Contains(search) ||
                                        c.Description.Contains(search));
                }
            }

            query = ApplyFilters(query,
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
                    dueDate);

            query = ApplyAgeFilters(query, statusDaysMin, openDaysMin);

            query = ApplyBacklogSort(query, sortColumn, sortDirection);

            return query;
        }, pageIndex, pageSize);
    }

    public async Task<IPagedList<Backlog>> GetPagedListForPivotAsync(int userId,
        int? lastAccessedProjectId,
        string? projectName,
        string? moduleName,
        string? subModuleName,
        string? severityName,
        string? releaseName,
        string? reporterName,
        string? developerName,
        string? testerName,
        string? taskTypeName,
        string? statusName,
        string? search = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        string sortColumn = "",
        string sortDirection = "")
    {
        var query = _backlogRepository.Table;

        query = query.Where(x => !x.Deleted);

        if (!string.IsNullOrEmpty(projectName))
            query = query.Where(x => x.Project.Name == projectName);
        else if (lastAccessedProjectId != null && lastAccessedProjectId > 0)
            query = query.Where(x => x.ProjectId == lastAccessedProjectId);
        else
        {
            var accessibleProjects = await _userService.GetAllAccessibleProjects(userId);
            var accessibleProjectsIds = accessibleProjects.Select(x => x.Id);
            query = query.Where(x => accessibleProjectsIds.Contains(x.ProjectId));
        }

        if (!string.IsNullOrEmpty(moduleName))
            query = query.Where(x => x.Module.Name == moduleName);

        if (!string.IsNullOrEmpty(subModuleName))
            query = query.Where(x => x.SubModule.Name == subModuleName);

        if (!string.IsNullOrEmpty(severityName))
            query = query.Where(x => x.Severity.Name == severityName);

        if (!string.IsNullOrEmpty(releaseName))
            query = query.Where(x => x.Release.Name == releaseName);

        if (!string.IsNullOrEmpty(reporterName))
            query = query.Where(x => x.Reporter.Name == reporterName);

        if (!string.IsNullOrEmpty(developerName))
            query = query.Where(x => developerName == (x.Assignee.FriendlyName == null || x.Assignee.FriendlyName == "" ? x.Assignee.FirstName + " " + x.Assignee.LastName : x.Assignee.FriendlyName));

        if (!string.IsNullOrEmpty(testerName))
            query = query.Where(x => testerName == (x.Tester.FriendlyName == null || x.Tester.FriendlyName == "" ? x.Tester.FirstName + " " + x.Tester.LastName : x.Tester.FriendlyName));

        if (!string.IsNullOrEmpty(taskTypeName))
            query = query.Where(x => x.TaskType.Name == taskTypeName);

        if (!string.IsNullOrEmpty(statusName))
            query = query.Where(x => x.Status.Name == statusName);

        if (!string.IsNullOrEmpty(search))
        {
            if (search.StartsWith("#"))
            {
                var idPart = search.Substring(1);
                if (int.TryParse(idPart, out int id))
                {
                    query = query.Where(x => x.Id == id);
                }
            }
            else
            {
                query = query.Where(c => c.Title.Contains(search) ||
                                    c.Description.Contains(search));
            }
        }

        query = query.ApplySafeSort(sortColumn, sortDirection);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public async Task<IList<Backlog>> GetAllForExcelExportAsync(int assigneeId,
        int projectId,
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
        int? dueDateType,
        DateOnly? dueDate,
        int[] accessiableProjects,
        string[] groupByColumns,
        bool canViewOthersTask = false,
        TaskSource? source = null,
        int[] project = null,
        int[] viewAllProjects = null,
        int? statusDaysMin = null,
        int? openDaysMin = null)
    {
        return await _backlogRepository.GetAllAsync(query =>
        {
            query = query.Where(x => !x.Deleted);
            if (source != null)
                query = query.Where(x => x.SourceId == (int)source);

            if (project != null && project.Length > 0)
                query = query.Where(x => project.Contains(x.ProjectId));
            else if (projectId > 0)
                query = query.Where(x => x.ProjectId == projectId);
            else
                query = query.Where(x => accessiableProjects.Contains(x.ProjectId));

            if (viewAllProjects != null)
                query = query.Where(x => viewAllProjects.Contains(x.ProjectId) || x.AssigneeId == assigneeId || x.TesterId == assigneeId);
            else if (projectId > 0 && !canViewOthersTask)
                query = query.Where(x => x.AssigneeId == assigneeId || x.TesterId == assigneeId);

            query = ApplyFilters(query,
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
                    dueDate);

            query = ApplyAgeFilters(query, statusDaysMin, openDaysMin);

            if (groupByColumns.Any())
                query = query.OrderBy(string.Join(",", groupByColumns));

            return query;
        }, false);
    }

    public async Task<IList<Backlog>> GetAllAsync(int projectId)
    {
        return await _backlogRepository.GetAllAsync(query =>
        {
            query = query.Where(x => !x.Deleted);

            if (projectId > 0)
                query = query.Where(x => x.ProjectId == projectId);

            return query;
        }, false);
    }

    public async Task<IPagedList<Backlog>> GetBoardCardsAsync(int projectId, int statusId, int pageIndex, int pageSize, int userId, bool canViewOthers)
    {
        return await _backlogRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => !x.Deleted && x.ProjectId == projectId && x.StatusId == statusId);

            if (!canViewOthers)
                query = query.Where(x => x.AssigneeId == userId || x.TesterId == userId);

            return query.OrderByDescending(x => x.CreatedOn);
        }, pageIndex, pageSize);
    }

    public async Task<IList<Backlog>> GetTimelineTasksAsync(int projectId, DateOnly from, DateOnly to, int userId, bool canViewOthers)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue);
        var toDt = to.ToDateTime(TimeOnly.MaxValue);

        return await _backlogRepository.GetAllAsync(query =>
        {
            query = query.Where(x => !x.Deleted && x.ProjectId == projectId &&
                ((x.DueDate != null && x.DueDate >= from && x.DueDate <= to)
                    || (x.PlannedStartDate != null && x.PlannedStartDate >= from && x.PlannedStartDate <= to)
                    || (x.DueDate == null && x.PlannedStartDate == null && x.CreatedOn >= fromDt && x.CreatedOn <= toDt)));

            if (!canViewOthers)
                query = query.Where(x => x.AssigneeId == userId || x.TesterId == userId);

            return query;
        }, false);
    }

    public async Task<IList<Backlog>> GetAllByReleaseAsync(int releaseId)
    {
        return await _backlogRepository.GetAllAsync(query =>
        {
            query = query.Where(x => !x.Deleted && x.ReleaseId == releaseId);
            return query;
        }, false);
    }

    public async Task<IList<Backlog>> GetReleaseProjectMismatchesAsync(int[] projectIds)
    {
        if (projectIds == null || projectIds.Length == 0)
            return [];

        return await _backlogRepository.GetAllAsync(query => query
            .Where(x => !x.Deleted
                && x.ReleaseId != null
                && projectIds.Contains(x.ProjectId)
                && x.Release.ReleaseProjects.Any()
                && !x.Release.ReleaseProjects.Any(p => p.ProjectId == x.ProjectId))
            .Include(x => x.Project)
            .Include(x => x.Release)
            .Include(x => x.Status)
            .OrderBy(x => x.Release.Name).ThenBy(x => x.Id), false);
    }

    public async Task<IList<ReleaseProjectProgressDto>> GetReleaseProjectProgressAsync(int[] releaseIds, int userId, int[] accessibleProjects, int[] viewAllProjects)
    {
        if (releaseIds == null || releaseIds.Length == 0)
            return [];

        var resolved = StatusGroup.Resolved.ToInt();
        return await _backlogRepository.Table
            .Where(x => !x.Deleted && x.ReleaseId != null && releaseIds.Contains(x.ReleaseId.Value))
            .Where(x => accessibleProjects.Contains(x.ProjectId))
            .Where(x => viewAllProjects.Contains(x.ProjectId) || x.AssigneeId == userId || x.TesterId == userId)
            .GroupBy(x => new { ReleaseId = x.ReleaseId!.Value, x.ProjectId, ProjectName = x.Project.Name })
            .Select(g => new ReleaseProjectProgressDto
            {
                ReleaseId = g.Key.ReleaseId,
                ProjectId = g.Key.ProjectId,
                Project = g.Key.ProjectName,
                Total = g.Count(),
                Completed = g.Count(x => x.Status.GroupId == resolved)
            })
            .ToListAsync();
    }

    public async Task<IList<Backlog>> GetBySourceWikiDocumentAsync(int wikiDocumentId)
    {
        return await _backlogRepository.GetAllAsync(query =>
            query.Where(x => !x.Deleted && x.SourceWikiDocumentId == wikiDocumentId)
                .OrderByDescending(x => x.Id), false);
    }

    public async Task<Backlog> GetByIdAsync(int id)
    {
        return id == 0 ? null : await _backlogRepository.GetByIdAsync(id, false,
            i => i.Project,
            i => i.Module,
            i => i.SubModule,
            i => i.TaskType,
            i => i.Severity,
            i => i.Assignee);
    }

    public async Task<string> UpdateFieldsAsync(int id, IList<FieldChange> changes)
    {
        try
        {
            // Load WITHOUT eager reference navigations. UpdateEntityProperty changes only the
            // scalar FKs; if a matching navigation (e.g. Assignee) were tracked, EF relationship
            // fixup would overwrite the new FK with the stale navigation's key on SaveChanges
            // (reverting the change). Lazy-loading proxies still populate navs on demand for the
            // notification reads below, reflecting the new values.
            var entity = await _backlogRepository.GetByIdAsync(id);
            if (entity == null)
                return "Error: Task not found.";

            var loggedUser = await _workContext.GetCurrentUserAsync();

            if (!await CanEditTaskAsync(entity, loggedUser.Id))
                return "Error: You are not allowed to edit this task.";

            var projectChange = changes.FirstOrDefault(c => c.Name == nameof(Backlog.ProjectId));
            if (projectChange != null && projectChange.Value.ToInt() != entity.ProjectId
                && !await CanChangeProjectAsync(entity, loggedUser.Id))
                return "Error: You are not allowed to change the project of this task.";

            var scopeError = await ValidateProjectScopeOnEditAsync(entity, changes);
            if (scopeError != null)
                return "Error: " + scopeError;

            var closingError = await ValidateClosingOnEditAsync(entity, changes);
            if (closingError != null)
                return "Error: " + closingError;

            var notifications = new List<ChatTaskUpdateDto>();
            var statusChanged = false;
            var assigneeChanged = false;
            var testerChanged = false;
            var statusValue = 0;
            var statusOldNew = string.Empty;

            foreach (var change in changes)
            {
                var property = change.Name;
                var value = change.Value;

                if (property == nameof(Backlog.StatusId) && value.ToInt() == (int)StatusGroup.ReOpened &&
                    _workItemSettings.MaxReopenCount > 0 && entity.ReOpenCount >= _workItemSettings.MaxReopenCount)
                    return $"Error: Reopen limit of {_workItemSettings.MaxReopenCount} reached.";

                if (property == nameof(Backlog.AssigneeId) && !_workItemSettings.AllowSelfAssign &&
                    value.ToInt() == loggedUser.Id)
                    return "Error: You cannot assign a task to yourself.";

                var cleanedPropertyName = CleanPropertyName(property);
                if (property == nameof(Backlog.ParentId))
                    cleanedPropertyName = nameof(Backlog.ParentId);

                var oldValue = CommonHelper.GetPropertyValue(entity, cleanedPropertyName);

                UpdateEntityProperty(entity, property, value);

                if (property == nameof(Backlog.StatusId) && value.ToInt() == (int)StatusGroup.ReOpened)
                    entity.ReOpenCount++;

                // FK fields display via a nav property that EF won't reload after the FK change,
                // so resolve the new name from the new id directly (else old == new in the history).
                var newValue = await ResolveFkNameAsync(property, value)
                    ?? CommonHelper.GetPropertyValue(entity, cleanedPropertyName);

                await _backlogCommentRepository.InsertAsync(BuildChangeComment(id, loggedUser.Id, property, oldValue, newValue));

                if (property == nameof(Backlog.StatusId))
                {
                    statusChanged = true;
                    statusValue = value.ToInt();
                    statusOldNew = $"{oldValue} → {newValue}";
                }
                if (property == nameof(Backlog.AssigneeId))
                    assigneeChanged = true;
                if (property == nameof(Backlog.TesterId))
                    testerChanged = true;
            }

            entity.ModifiedById = loggedUser.Id;
            entity.ModifiedOn = DateTime.Now;

            await _backlogRepository.UpdateAsync(entity);

            if (statusChanged)
            {
                await _backlogStatusLogRepository.InsertAsync(new BacklogStatusLog
                {
                    BacklogId = id,
                    StatusId = statusValue,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.Now
                });

                await AddToNotification(ChatEvent.StatusChanged, entity, notifications, statusOldNew);
            }

            if (assigneeChanged && entity.AssigneeId > 0)
            {
                var newEntity = await _backlogRepository.GetByIdAsync(entity.Id);
                await _messageService.EmailNewTaskNotificationAsync(newEntity, false);

                await _notificationChannelService.TaskAssignedAsync(newEntity, false);
            }

            if (testerChanged && entity.TesterId > 0)
            {
                var newEntity = await _backlogRepository.GetByIdAsync(entity.Id);
                await _messageService.EmailNewTaskNotificationAsync(newEntity, true);

                await _notificationChannelService.TaskAssignedAsync(newEntity, true);
            }

            if (notifications.Count > 0)
                await _notificationChannelService.TaskUpdatesAsync(notifications);

            return "Saved";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public async Task<IList<Backlog>> GetOpenAssignmentsAsync()
    {
        var openGroups = new[] { StatusGroup.New.ToInt(), StatusGroup.InProgress.ToInt(), StatusGroup.ReOpened.ToInt() };

        var query = _backlogRepository.Table
            .Include(t => t.Project)
            .Include(t => t.Status)
            .Include(t => t.Assignee)
            .Include(t => t.Tester)
            .Where(t => !t.Deleted
                && t.Project.Active && !t.Project.Deleted
                && openGroups.Contains(t.Status.GroupId)
                && (t.AssigneeId != null || t.TesterId != null))
            .OrderBy(t => t.ProjectId)
            .ThenBy(t => t.DueDate);

        return await query.ToListAsync();
    }

    public async Task<IList<ProjectOpenStatusSummaryDto>> GetOpenStatusSummaryAsync()
    {
        var openGroups = new[] { StatusGroup.New.ToInt(), StatusGroup.InProgress.ToInt(), StatusGroup.ReOpened.ToInt() };

        var rows = await _backlogRepository.Table
            .Where(t => !t.Deleted && t.Project.Active && !t.Project.Deleted && openGroups.Contains(t.Status.GroupId))
            .GroupBy(t => new
            {
                t.ProjectId,
                ProjectName = t.Project.Name,
                t.Project.ProjectManagerId,
                t.Project.TeamLeadId,
                StatusName = t.Status.Name,
                t.Status.GroupId
            })
            .Select(g => new
            {
                g.Key.ProjectId,
                g.Key.ProjectName,
                g.Key.ProjectManagerId,
                g.Key.TeamLeadId,
                g.Key.StatusName,
                g.Key.GroupId,
                Count = g.Count()
            })
            .ToListAsync();

        return rows
            .GroupBy(r => new { r.ProjectId, r.ProjectName, r.ProjectManagerId, r.TeamLeadId })
            .Select(pg => new ProjectOpenStatusSummaryDto
            {
                ProjectId = pg.Key.ProjectId,
                ProjectName = pg.Key.ProjectName,
                ProjectManagerId = pg.Key.ProjectManagerId,
                TeamLeadId = pg.Key.TeamLeadId,
                TotalOpen = pg.Sum(x => x.Count),
                Statuses = pg.OrderBy(x => x.GroupId).ThenBy(x => x.StatusName)
                    .Select(x => new StatusCount { StatusName = x.StatusName, Count = x.Count })
                    .ToList()
            })
            .OrderBy(p => p.ProjectName)
            .ToList();
    }

    public async Task<IList<Backlog>> SearchSimilarInProjectAsync(int projectId, string title, int take = 5)
    {
        if (string.IsNullOrWhiteSpace(title))
            return [];

        return await _backlogRepository.Table
            .Where(x => !x.Deleted && x.ProjectId == projectId && x.Title.Contains(title))
            .OrderByDescending(x => x.Id)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IList<Backlog>> SearchSimilarAsync(int[] projectIds, string title, int take = 8)
    {
        if (string.IsNullOrWhiteSpace(title) || projectIds == null || projectIds.Length == 0)
            return [];

        var trimmed = title.Trim();

        return await _backlogRepository.Table
            .Include(x => x.Project)
            .Include(x => x.TaskType)
            .Include(x => x.Status)
            .Where(x => !x.Deleted && projectIds.Contains(x.ProjectId) && x.Title.Contains(trimmed))
            .OrderByDescending(x => x.Title == trimmed)
            .ThenByDescending(x => x.Id)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IList<Backlog>> SearchByTitleAsync(string search, int excludeTaskId = 0)
    {
        var query = _backlogRepository.Table;

        query = query.Where(x => !x.Deleted);

        if (excludeTaskId > 0)
            query = query.Where(x => x.Id != excludeTaskId);

        bool isNumber = int.TryParse(search, out int number);
        if (isNumber)
            query = query.Where(x => x.Id == number);
        else
            query = query.Where(x => x.Title.Contains(search));

        return await query.ToListAsync();
    }

    public async Task<IList<Backlog>> GetChildrenAsync(int parentId, int userId, int[] accessibleProjects, int[] viewAllProjects)
    {
        var query = _backlogRepository.Table
            .Where(x => !x.Deleted && x.ParentId == parentId);

        if (accessibleProjects != null && accessibleProjects.Length > 0)
            query = query.Where(x => accessibleProjects.Contains(x.ProjectId));

        if (viewAllProjects != null)
            query = query.Where(x => viewAllProjects.Contains(x.ProjectId) || x.AssigneeId == userId || x.TesterId == userId);

        return await query.OrderBy(x => x.Id).Take(200).ToListAsync();
    }

    public async Task<Dictionary<int, int>> GetChildCountsAsync(int[] parentIds)
    {
        if (parentIds == null || parentIds.Length == 0)
            return new Dictionary<int, int>();

        return await _backlogRepository.Table
            .Where(x => !x.Deleted && x.ParentId != null && parentIds.Contains(x.ParentId.Value))
            .GroupBy(x => x.ParentId.Value)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId, x => x.Count);
    }

    public async Task<Dictionary<int, (DateTime First, DateTime Last)>> GetStatusChangeBoundsAsync(int[] taskIds)
    {
        if (taskIds == null || taskIds.Length == 0)
            return new Dictionary<int, (DateTime First, DateTime Last)>();

        var rows = await _backlogStatusLogRepository.Table
            .Where(x => taskIds.Contains(x.BacklogId))
            .GroupBy(x => x.BacklogId)
            .Select(g => new { TaskId = g.Key, First = g.Min(x => x.CreatedOn), Last = g.Max(x => x.CreatedOn) })
            .ToListAsync();

        return rows.ToDictionary(x => x.TaskId, x => (x.First, x.Last));
    }

    public async Task InsertAsync(Backlog entity, List<int> documentIds)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        entity.Code = Guid.NewGuid();
        entity.Parent = null;
        await _backlogRepository.InsertAsync(entity);
        if (entity.Id > 0)
        {
            if (documentIds.Count > 0)
            {
                var documentEntities = new List<BacklogDocument>();
                foreach (var id in documentIds)
                {
                    documentEntities.Add(new BacklogDocument
                    {
                        BacklogId = entity.Id,
                        DocumentId = id,
                        CreatedById = entity.CreatedById,
                        CreatedOn = entity.CreatedOn
                    });
                }

                await _backlogDocumentRepository.InsertAsync(documentEntities);
            }

            var taskType = await _taskTypeService.GetByIdAsync(entity.TaskTypeId);

            await _backlogCommentRepository.InsertAsync(new BacklogComment
            {
                BacklogId = entity.Id,
                CreatedById = entity.CreatedById,
                CreatedOn = DateTime.UtcNow,
                Comment = $"New {taskType.Name} created.",
                SystemComment = true
            });

            await _backlogStatusLogRepository.InsertAsync(new BacklogStatusLog
            {
                BacklogId = entity.Id,
                StatusId = entity.StatusId,
                CreatedById = entity.CreatedById,
                CreatedOn = DateTime.Now
            });
        }
    }

    public async Task UpdateAsync(Backlog entity)
    {

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _backlogRepository.UpdateAsync(entity);
    }

    public async Task<string> UpdateAsync(int id, string property, string value)
    {
        string message = string.Empty;
        try
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var editor = await _workContext.GetCurrentUserAsync();
            if (!await CanEditTaskAsync(entity, editor.Id))
                return "Error: You are not allowed to edit this task.";

            if (property == nameof(Backlog.StatusId) && value.ToInt() == (int)StatusGroup.ReOpened &&
                _workItemSettings.MaxReopenCount > 0 && entity.ReOpenCount >= _workItemSettings.MaxReopenCount)
            {
                return $"Error: Reopen limit of {_workItemSettings.MaxReopenCount} reached.";
            }

            if (property == nameof(Backlog.AssigneeId) && !_workItemSettings.AllowSelfAssign)
            {
                var requester = await _workContext.GetCurrentUserAsync();
                if (requester != null && value.ToInt() == requester.Id)
                    return "Error: You cannot assign a task to yourself.";
            }

            var scopeError = await ValidateProjectScopeOnEditAsync(entity, [new FieldChange { Name = property, Value = value }]);
            if (scopeError != null)
                return "Error: " + scopeError;

            var closingError = await ValidateClosingOnEditAsync(entity, [new FieldChange { Name = property, Value = value }]);
            if (closingError != null)
                return "Error: " + closingError;

            var cleanedPropertyName = CleanPropertyName(property);
            if (property == nameof(Backlog.ParentId))
                cleanedPropertyName = nameof(Backlog.ParentId);

            var oldValue = CommonHelper.GetPropertyValue(entity, cleanedPropertyName);

            var updatedEntity = UpdateEntityProperty(entity, property, value);

            if (property == nameof(Backlog.StatusId) && value.ToInt() == (int)StatusGroup.ReOpened)
            {
                entity.ReOpenCount++;
            }

            var loggedUser = await _workContext.GetCurrentUserAsync();

            entity.ModifiedById = loggedUser.Id;
            entity.ModifiedOn = DateTime.Now;

            await _backlogRepository.UpdateAsync(entity);

            var newValue = await ResolveFkNameAsync(property, value)
                ?? CommonHelper.GetPropertyValue(entity, cleanedPropertyName);

            message = $"{CommonHelper.Beautify(property)} updated!";

            await _backlogCommentRepository.InsertAsync(BuildChangeComment(id, loggedUser.Id, property, oldValue, newValue));

            if (property == nameof(Backlog.StatusId))
            {
                await _backlogStatusLogRepository.InsertAsync(new BacklogStatusLog
                {
                    BacklogId = id,
                    StatusId = value.ToInt(),
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.Now
                });

                var notifications = new List<ChatTaskUpdateDto>();
                await AddToNotification(ChatEvent.StatusChanged, entity, notifications, $"Status: {oldValue} → {newValue}");
                await _notificationChannelService.TaskUpdatesAsync(notifications);
            }

            if (property == nameof(Backlog.AssigneeId) && entity.Assignee != null &&
                entity.AssigneeId > 0)
            {
                var newEntity = await _backlogRepository.GetByIdAsync(entity.Id);
                await _messageService.EmailNewTaskNotificationAsync(newEntity, false);

                await _notificationChannelService.TaskAssignedAsync(newEntity, false);
            }
            if (property == nameof(Backlog.TesterId) && entity.Tester != null &&
                entity.TesterId > 0)
            {
                var newEntity = await _backlogRepository.GetByIdAsync(entity.Id);
                await _messageService.EmailNewTaskNotificationAsync(newEntity, true);

                await _notificationChannelService.TaskAssignedAsync(newEntity, true);
            }
        }
        catch (Exception ex)
        {
            message = $"Error: {ex.Message}";
        }

        return message;
    }

    public async Task<List<int>> GetProjectIdsByTaskIdsAsync(int[] ids)
    {
        return await _backlogRepository.Table
            .Where(x => ids.Contains(x.Id))
            .Select(x => x.ProjectId)
            .Distinct()
            .ToListAsync();
    }

    public async Task UpdateBatchAsync(string ids, int? taskTypeId, int? reporterId, int? severityId,
        DateOnly? dueDate, int? moduleId, int? subModuleId, int? releaseId, int? assigneeId, int? testerId, int? statusId)
    {
        var updateAvaliable = false;
        var taskIdsRaw = ids.Split([','], StringSplitOptions.RemoveEmptyEntries);
        int[] taskIds = taskIdsRaw.Select(int.Parse).ToArray();
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var tasks = await _backlogRepository.GetByIdsAsync(taskIds);
        var comments = new List<BacklogComment>();
        var backlogStatus = new List<BacklogStatusLog>();
        var notifications = new List<ChatTaskUpdateDto>();

        foreach (var task in tasks)
        {
            if (assigneeId != null && assigneeId > 0 && task.AssigneeId != assigneeId)
            {
                var oldValue = task.AssigneeId != null ? task.Assignee.Name : "N/A";
                var newValue = await _userService.GetByIdAsync(assigneeId.ToInt());
                var comment = $"Assignee: {oldValue} → {newValue.Name}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.AssigneeId = assigneeId.ToInt();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (assigneeId == 0 && task.AssigneeId != null)
            {
                var comment = $"Assignee: {task.Assignee.Name} → Unassigned";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.AssigneeId = null;
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (testerId == 0 && task.TesterId != null)
            {
                var comment = $"Tester: {task.Tester.Name} → Unassigned";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.TesterId = null;
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (testerId != null && testerId > 0 && task.TesterId != testerId)
            {
                var oldValue = task.TesterId != null ? task.Tester.Name : "N/A";
                var newValue = await _userService.GetByIdAsync(testerId.ToInt());
                var comment = $"Tester: {oldValue} → {newValue.Name}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.TesterId = testerId.ToInt();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (taskTypeId != null && taskTypeId > 0 && task.TaskTypeId != taskTypeId)
            {
                var newValue = await _taskTypeService.GetByIdAsync(taskTypeId.ToInt());
                var comment = $"Task Type: {task.TaskType.Name} → {newValue.Name}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.TaskTypeId = taskTypeId.ToInt();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (reporterId != null && reporterId > 0 && task.ReporterId != reporterId)
            {
                var oldValue = task.ReporterId != null ? task.Reporter.Name : "N/A";
                var newValue = await _repoterService.GetByIdAsync(reporterId.ToInt());
                var comment = $"Reporter: {oldValue} → {newValue.Name}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.ReporterId = reporterId.ToInt();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (severityId != null && severityId > 0 && task.SeverityId != severityId)
            {
                var newValue = await _severityService.GetByIdAsync(severityId.ToInt());
                var comment = $"Severity: {task.Severity.Name} → {newValue.Name}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.SeverityId = severityId.ToInt();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (dueDate != null && task.DueDate != dueDate)
            {
                var oldValue = task.DueDate != null ? task.DueDate.ToDateString("dd-MM-yyyy") : "N/A";
                var comment = $"Due Date: {oldValue} → {dueDate}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.DueDate = dueDate.ToDateOnly();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (moduleId != null && moduleId > 0 && task.ModuleId != moduleId)
            {
                var oldValue = task.ModuleId != null ? task.Module.Name : "N/A";
                var newValue = await _moduleService.GetByIdAsync(moduleId.ToInt());
                var comment = $"Module: {oldValue} → {newValue.Name}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.ModuleId = moduleId.ToInt();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (subModuleId != null && subModuleId > 0 && task.SubModuleId != subModuleId)
            {
                var oldValue = task.SubModuleId != null ? task.SubModule.Name : "N/A";
                var newValue = await _subModuleService.GetByIdAsync(subModuleId.ToInt());
                var comment = $"Sub-Module: {oldValue} → {newValue.Name}";

                comments.Add(new()
                {
                    BacklogId = task.Id,
                    CreatedById = loggedUser.Id,
                    CreatedOn = DateTime.UtcNow,
                    Comment = comment,
                    SystemComment = true
                });

                task.SubModuleId = subModuleId.ToInt();
                updateAvaliable = true;

                await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
            }

            if (releaseId != null && releaseId >= 0 && task.ReleaseId != releaseId)
            {
                var releaseUpdateAvaliable = false;
                var oldValue = task.ReleaseId != null ? task.Release.Name : "N/A";
                var newValue = "N/A";
                var comment = $"Release: {oldValue} → {newValue}";



                if (releaseId > 0)
                {
                    var newRelease = await _releaseService.GetByIdAsync(releaseId.ToInt());
                    newValue = newRelease.Name;
                }

                if (releaseId == 0)
                {
                    task.ReleaseId = null;
                    updateAvaliable = true;
                    releaseUpdateAvaliable = true;
                }
                else if (releaseId > 0)
                {
                    task.ReleaseId = releaseId;
                    updateAvaliable = true;
                    releaseUpdateAvaliable = true;
                }

                if (releaseUpdateAvaliable)
                {
                    comments.Add(new()
                    {
                        BacklogId = task.Id,
                        CreatedById = loggedUser.Id,
                        CreatedOn = DateTime.UtcNow,
                        Comment = comment,
                        SystemComment = true
                    });
                    await AddToNotification(ChatEvent.TaskUpdated, task, notifications, comment);
                }
            }

            if (statusId != null && statusId > 0 && task.StatusId != statusId)
            {
                var newTaskType = task.TaskType;
                if (taskTypeId != null && taskTypeId > 0)
                {
                    newTaskType = await _taskTypeService.GetByIdAsync(taskTypeId.ToInt());
                }

                var newStatus = await _statusService.GetByIdAsync(statusId.ToInt());

                var closing = await _closingRequirementService.EvaluateAsync(task, newStatus, newTaskType?.Id);
                if (!closing.AnyMissing)
                {
                    var comment = $"Status: {task.Status.Name} → {newStatus.Name}";

                    comments.Add(new()
                    {
                        BacklogId = task.Id,
                        CreatedById = loggedUser.Id,
                        CreatedOn = DateTime.UtcNow,
                        Comment = comment,
                        SystemComment = true
                    });

                    backlogStatus.Add(new BacklogStatusLog
                    {
                        BacklogId = task.Id,
                        StatusId = statusId.ToInt(),
                        CreatedById = loggedUser.Id,
                        CreatedOn = DateTime.Now
                    });

                    task.StatusId = statusId.ToInt();
                    updateAvaliable = true;

                    await AddToNotification(ChatEvent.StatusChanged, task, notifications, comment);
                }
            }
        }

        if (updateAvaliable)
        {
            await _backlogRepository.UpdateAsync(tasks);
            await _backlogCommentRepository.InsertAsync(comments);
            if (notifications.Any())
                await _notificationChannelService.TaskUpdatesAsync(notifications);

            await _backlogStatusLogRepository.InsertAsync(backlogStatus);
        }
    }

    public async Task UpdateStatusOnMergeActionAsync(GitMergeRequestInfo mergeRequest, User? actor, string? actorUserName, IList<int> taskIds,
        int? repositoryId, string mergeRequestLabel)
    {
        if (taskIds == null || taskIds.Count == 0)
            return;

        var mergeState = mergeRequest.State?.ToLowerInvariant() ?? string.Empty;
        var status = mergeState switch
        {
            "merged" => await ResolveMergeStatusAsync(_integrationSettings.MergedStatusId, MergeStatusDefaults.Merged),
            "closed" => await ResolveMergeStatusAsync(_integrationSettings.RejectedStatusId, MergeStatusDefaults.Rejected),
            _ => null
        };

        {
            if (status != null && status.Id > 0)
            {
                var backlogStatus = new List<BacklogStatusLog>();
                var comments = new List<BacklogComment>();

                var backlogItems = (await _backlogRepository.GetByIdsAsync(taskIds.Distinct().ToList()))
                    .Where(x => !x.Deleted && x.StatusId != status.Id)
                    .ToList();
                var byWhom = actor == null && !string.IsNullOrWhiteSpace(actorUserName) ? $" by @{actorUserName}" : string.Empty;

                var mrIid = mergeRequest.Iid;
                var closedGroups = new[] { (int)StatusGroup.Resolved, (int)StatusGroup.Archived };

                foreach (var item in backlogItems.ToList())
                {
                    var oldValue = item.Status.Name;
                    var actorId = actor?.Id ?? item.AssigneeId ?? item.CreatedById;

                    if (mergeState == "merged" && mrIid > 0 && !(item.MergeRequestId > 0))
                    {
                        item.MergeRequestId = mrIid;
                        item.MergeRequestRepositoryId = repositoryId;
                    }

                    if (closedGroups.Contains(item.Status.GroupId))
                    {
                        comments.Add(new BacklogComment
                        {
                            BacklogId = item.Id,
                            CreatedById = actorId,
                            CreatedOn = DateTime.UtcNow,
                            Comment = $"{mergeRequestLabel} was {mergeState}{byWhom}; status left as {oldValue} because the task is already closed",
                            SystemComment = true
                        });
                        continue;
                    }

                    comments.Add(new BacklogComment
                    {
                        BacklogId = item.Id,
                        CreatedById = actorId,
                        CreatedOn = DateTime.UtcNow,
                        Comment = $"Status: {oldValue} → {status.Name} via Git merge ({mergeRequestLabel}){byWhom}",
                        SystemComment = true
                    });

                    backlogStatus.Add(new BacklogStatusLog
                    {
                        BacklogId = item.Id,
                        StatusId = status.Id,
                        CreatedById = actorId,
                        CreatedOn = DateTime.Now
                    });

                    item.StatusId = status.Id;
                }

                await _backlogRepository.UpdateAsync(backlogItems);

                if (comments.Any())
                    await _backlogCommentRepository.InsertAsync(comments);

                if (backlogStatus.Any())
                    await _backlogStatusLogRepository.InsertAsync(backlogStatus);
            }
        }
    }

    public async Task<int> MoveTasksForOpenedMergeRequestAsync(IList<int> taskIds, int mergeRequestIid, User? actor, string? actorGitUserName,
        int? repositoryId, string mergeRequestLabel)
    {
        if (!_integrationSettings.MoveTasksOnMergeRequestOpen || taskIds == null || taskIds.Count == 0)
            return 0;

        var status = await ResolveMergeStatusAsync(_integrationSettings.OpenedStatusId, MergeStatusDefaults.Opened);
        if (status == null)
            return 0;

        var items = (await _backlogRepository.GetByIdsAsync(taskIds.Distinct().ToList()))
            .Where(x => !x.Deleted && x.StatusId != status.Id)
            .ToList();
        if (items.Count == 0)
            return 0;

        var byWhom = actor != null ? $" by {actor.Name}" : !string.IsNullOrWhiteSpace(actorGitUserName) ? $" by @{actorGitUserName}" : string.Empty;
        var closedGroups = new[] { (int)StatusGroup.Resolved, (int)StatusGroup.Archived };

        var comments = new List<BacklogComment>();
        var logs = new List<BacklogStatusLog>();
        var moved = 0;

        foreach (var item in items)
        {
            var actorId = actor?.Id ?? item.AssigneeId ?? item.CreatedById;
            var oldValue = item.Status.Name;

            if (closedGroups.Contains(item.Status.GroupId))
            {
                comments.Add(new BacklogComment
                {
                    BacklogId = item.Id,
                    CreatedById = actorId,
                    CreatedOn = DateTime.UtcNow,
                    Comment = $"{mergeRequestLabel} opened{byWhom}; status left as {oldValue} because the task is already closed",
                    SystemComment = true
                });
                continue;
            }

            var group = (TaskTypeGroup)(item.TaskType?.GroupId ?? 0);
            if (!(item.MergeRequestId > 0) && _closingRequirementService.IsRequired(status, group, ClosingItem.MergeRequest))
            {
                item.MergeRequestId = mergeRequestIid;
                item.MergeRequestRepositoryId = repositoryId;
            }

            comments.Add(new BacklogComment
            {
                BacklogId = item.Id,
                CreatedById = actorId,
                CreatedOn = DateTime.UtcNow,
                Comment = $"Status: {oldValue} → {status.Name} via {mergeRequestLabel} opened{byWhom}",
                SystemComment = true
            });
            logs.Add(new BacklogStatusLog
            {
                BacklogId = item.Id,
                StatusId = status.Id,
                CreatedById = actorId,
                CreatedOn = DateTime.Now
            });

            item.StatusId = status.Id;
            moved++;
        }

        await _backlogRepository.UpdateAsync(items);
        if (comments.Count > 0)
            await _backlogCommentRepository.InsertAsync(comments);
        if (logs.Count > 0)
            await _backlogStatusLogRepository.InsertAsync(logs);

        return moved;
    }

    public async Task DeleteAsync(Backlog entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _backlogRepository.DeleteAsync(entity);
    }

    #endregion

    #region Methods For Documents

    public async Task<IList<BacklogDocument>> GetAllDocumentAsync(int backlogId)
    {
        return await _backlogDocumentRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId);
            return query;
        }, false);
    }

    public async Task<BacklogDocument> GetAttachmentAsync(int id)
    {
        return id <= 0 ? null : await _backlogDocumentRepository.GetByIdAsync(id);
    }

    public async Task InsertDocumentAsync(int backlogId, int documentId, int loggedUserId)
    {
        var entity = await GetByIdAsync(backlogId);

        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (entity.Id > 0 && documentId > 0)
        {
            var docEntity = new BacklogDocument
            {
                BacklogId = entity.Id,
                DocumentId = documentId,
                CreatedById = loggedUserId,
                CreatedOn = DateTime.Now
            };
            await _backlogDocumentRepository.InsertAsync(docEntity);

            if (docEntity.Id > 0)
            {
                var backlog = await GetByIdAsync(backlogId);
                backlog.AttachmentCount = backlog.AttachmentCount == null
                    ? 1
                    : backlog.AttachmentCount + 1;

                await UpdateAsync(backlog);

                await _backlogCommentRepository.InsertAsync(new BacklogComment
                {
                    BacklogId = backlogId,
                    CreatedById = loggedUserId,
                    CreatedOn = DateTime.UtcNow,
                    Comment = $"Added file {docEntity.Document.FileName}",
                    SystemComment = true
                });
            }
        }
    }

    public async Task<bool> DeleteDocumentAsync(int documentId, int loggedUserId)
    {
        var entity = await _backlogDocumentRepository.GetByIdAsync(documentId);
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var backlog = await GetByIdAsync(entity.BacklogId);

        var commentEntity = new BacklogComment
        {
            BacklogId = entity.BacklogId,
            CreatedById = loggedUserId,
            CreatedOn = DateTime.UtcNow,
            Comment = $"Deleted file {entity.Document.FileName}",
            SystemComment = true
        };

        await _backlogDocumentRepository.DeleteAsync(entity);

        var entityExists = await _backlogDocumentRepository.GetByIdAsync(documentId);
        if (entityExists == null)
        {
            if (backlog.AttachmentCount != null && backlog.AttachmentCount > 0)
            {
                backlog.AttachmentCount = backlog.AttachmentCount - 1;
                await UpdateAsync(backlog);
            }

            await _backlogCommentRepository.InsertAsync(commentEntity);
            return true;
        }

        return false;
    }

    #endregion

    #region Methods For History

    public async Task<IList<BacklogComment>> GetAllHistoryAsync(int backlogId)
    {
        return await _backlogCommentRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId && x.SystemComment && x.Comment != "Added a comment").OrderByDescending(x => x.CreatedOn);
            return query;
        }, false);
    }

    #endregion

    #region Methods For Comments

    public async Task<IList<BacklogComment>> GetAllCommentsAsync(int backlogId)
    {
        return await _backlogCommentRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId && !x.SystemComment).OrderByDescending(x => x.CreatedOn);
            return query;
        }, false);
    }

    public async Task InsertCommentAsync(int backlogId, string comment, int loggedUserId, string loggedUserName, bool postToIssue = false)
    {
        var entity = await GetByIdAsync(backlogId);

        if (backlogId <= 0)
            throw new ArgumentNullException(nameof(backlogId));

        var newComment = new BacklogComment
        {
            BacklogId = backlogId,
            Comment = comment,
            CreatedById = loggedUserId,
            CreatedOn = DateTime.Now,
            SystemComment = false,
            PostToIssue = postToIssue
        };

        await _backlogCommentRepository.InsertAsync(newComment);

        if (newComment.Id > 0)
        {
            var backlog = await GetByIdAsync(backlogId);
            backlog.CommentCount = backlog.CommentCount == null
                ? 1
                : backlog.CommentCount + 1;

            await UpdateAsync(backlog);

            await _backlogCommentRepository.InsertAsync(new BacklogComment
            {
                BacklogId = backlogId,
                CreatedById = loggedUserId,
                CreatedOn = DateTime.UtcNow,
                Comment = $"Added a comment",
                SystemComment = true
            });

            var notifications = new List<ChatTaskUpdateDto>();
            await AddToNotification(ChatEvent.Commented, backlog, notifications, $"A new comment was added by {loggedUserName}");
            await _notificationChannelService.TaskUpdatesAsync(notifications);
        }
    }

    #endregion

    #region Methods For Status History

    public async Task<IList<BacklogStatusLog>> GetStatusLogByTaskId(int taskId)
    {
        var query = _backlogStatusLogRepository.Table.Where(x => x.BacklogId == taskId);

        return await query.ToListAsync();
    }

    #endregion

    #region Methods For Reports

    public async Task<IDictionary<string, int>> GetTaskCountAsync(int userId, int[] projectIds = null)
    {
        var data = new Dictionary<string, int>();

        var projectsIds = (projectIds != null && projectIds.Length > 0)
            ? projectIds
            : (await _userService.GetAllAccessibleProjects(userId)).Select(x => x.Id).ToArray();
        var viewAllIds = (await _userService.GetAllAccessibleProjects(userId, fetchOnlyCanViewOthersTask: true)).Select(x => x.Id).ToArray();

        var visible = _backlogRepository.Table
            .Where(x => !x.Deleted && projectsIds.Contains(x.ProjectId) && x.Project.Active && !x.Project.Deleted)
            .Where(x => viewAllIds.Contains(x.ProjectId) || x.AssigneeId == userId || x.TesterId == userId);

        data.Add("Total Tasks", await visible.CountAsync());
        data.Add("My Tasks", await visible.CountAsync(x => x.AssigneeId == userId || x.TesterId == userId));
        data.Add("Reopened Tasks", await visible.CountAsync(x => x.AssigneeId == userId && x.ReOpenCount > 0));
        data.Add("Closed Tasks", await visible.CountAsync(x => x.AssigneeId == userId && x.Status.GroupId == (int)StatusGroup.Resolved));

        return data;
    }

    #endregion

    #region Methods For Custom Fields

    public async Task<IList<BacklogCustomFieldValue>> GetAllCustomFieldValuesAsync(int backlogId)
    {
        return await _backlogCustomFieldValueRepository.GetAllAsync(query =>
        {
            query = query.Where(x => x.BacklogId == backlogId);
            return query;
        }, false);
    }

    public async Task<BacklogCustomFieldValue> GetCustomFieldValueAsync(int backlogId, int customFieldId)
    {
        return await _backlogCustomFieldValueRepository.Table
            .Where(x => x.BacklogId == backlogId && x.CustomFieldId == customFieldId)
            .FirstOrDefaultAsync();
    }

    public async Task<string> InsertFieldValueAsync(int backlogId, int customFieldId, string value)
    {
        string message = string.Empty;

        try
        {
            if (backlogId <= 0 || customFieldId <= 0)
                throw new ArgumentNullException(nameof(backlogId));

            var existence = await _backlogCustomFieldValueRepository.Table
                .FirstOrDefaultAsync(x => x.BacklogId == backlogId && x.CustomFieldId == customFieldId);

            var customField = await _customFieldService.GetByIdAsync(customFieldId);
            var oldValue = string.Empty;

            if (existence == null)
            {
                await _backlogCustomFieldValueRepository.InsertAsync(new BacklogCustomFieldValue
                {
                    BacklogId = backlogId,
                    CustomFieldId = customFieldId,
                    Value = value
                });
            }
            else
            {
                oldValue = existence.Value;
                existence.Value = value;

                await _backlogCustomFieldValueRepository.UpdateAsync(existence);

                message = $"{CommonHelper.Beautify(customField.Label)} updated!";
            }


            var loggedUser = await _workContext.GetCurrentUserAsync();
            await _backlogCommentRepository.InsertAsync(new BacklogComment
            {
                BacklogId = backlogId,
                CreatedById = loggedUser.Id,
                CreatedOn = DateTime.UtcNow,
                Comment = $"{CommonHelper.Beautify(customField.Label)}: {oldValue} → {value}",
                SystemComment = true
            });
        }
        catch (Exception ex)
        {
            message = "Error";
        }

        return message;
    }

    public async Task InsertFieldValueAsync(BacklogCustomFieldValue entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _backlogCustomFieldValueRepository.InsertAsync(entity);
    }

    public async Task InsertFieldValueAsync(IList<BacklogCustomFieldValue> entities)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        await _backlogCustomFieldValueRepository.InsertAsync(entities);
    }

    #endregion

    #region Methods For Charts

    public async Task<List<ReleaseVsTaskDto>> GetReleaseTaskDataAsync(int assigneeId, int[] projectIds = null)
    {
        var hasScope = projectIds != null && projectIds.Length > 0;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var data = await _backlogRepository.Table.Where(x => x.AssigneeId == assigneeId && (!hasScope || projectIds.Contains(x.ProjectId)))
        .GroupBy(t => new { t.Release.Name, t.Release.ReleaseDate })
        .OrderBy(g => g.Key.ReleaseDate)
        .Take(3)
        .Select(g => new ReleaseVsTaskDto
        {
            ReleaseVersion = g.Key.Name,
            OpenTasks = g.Count(x => x.Status.GroupId != StatusGroup.Resolved.ToInt()),
            ClosedTasks = g.Count(x => x.Status.GroupId == StatusGroup.Resolved.ToInt())
        }).ToListAsync();

        return data;
    }

    public async Task<Dictionary<string, int>> GetDueTasksAsync(int assigneeId, int[] projectIds = null)
    {
        var hasScope = projectIds != null && projectIds.Length > 0;
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        var next7DaysEnd = today.AddDays(8);

        var data = _backlogRepository.Table.Where(x => x.AssigneeId == assigneeId && !x.Deleted && x.Project.Active && !x.Project.Deleted && (!hasScope || projectIds.Contains(x.ProjectId)))
            .AsEnumerable()
            .GroupBy(t =>
            {
                if (!t.DueDate.HasValue)
                    return "No Due Date";
                else if (t.DueDate.Value == DateOnly.FromDateTime(today))
                    return "Today";
                else if (t.DueDate.Value == DateOnly.FromDateTime(tomorrow))
                    return "Tomorrow";
                else if (t.DueDate.Value > DateOnly.FromDateTime(tomorrow) && t.DueDate.Value <= DateOnly.FromDateTime(next7DaysEnd))
                    return "Next 7 Days";
                else if (t.DueDate.Value < DateOnly.FromDateTime(today))
                    return "Overdue";
                else
                    return "Future";
            }).ToDictionary(g => g.Key, g => g.Count());

        return data;
    }

    public async Task<List<TaskCountDto>> GetProjectTasksAsync(int assigneeId, int[] projectIds = null)
    {
        var hasScope = projectIds != null && projectIds.Length > 0;
        var data = await _backlogRepository
                .Table.Where(x => x.AssigneeId == assigneeId && !x.Deleted && x.Project.Active && !x.Project.Deleted
                    && (x.Status.GroupId == (int)StatusGroup.New || x.Status.GroupId == (int)StatusGroup.InProgress || x.Status.GroupId == (int)StatusGroup.ReOpened)
                    && (!hasScope || projectIds.Contains(x.ProjectId)))
                .GroupBy(t => t.ProjectId)
                .Select(g => new TaskCountDto
                {
                    Name = g.FirstOrDefault().Project.Name,
                    TaskCount = g.Count()
                }).ToListAsync();

        return data;
    }

    #endregion

    #region Pivot

    public async Task<List<BacklogPivotDto>> GetPivotDataAsync(int projectId)
    {
        var query = _backlogRepository.Table;

        if (projectId > 0)
            query = query.Where(x => x.ProjectId == projectId);

        query = query.Where(x => x.Status.GroupId == StatusGroup.New.ToInt() ||
                            x.Status.GroupId == StatusGroup.InProgress.ToInt() ||
                            x.Status.GroupId == StatusGroup.ReOpened.ToInt());

        var data = await query.Select(x => new BacklogPivotDto
        {
            TaskType = x.TaskType.Name,
            Severity = x.Severity.Name,
            DueDate = x.DueDate,
            Project = x.Project.Name,
            Module = x.Module.Name,
            SubModule = x.SubModule.Name,
            Release = x.Release.Name,
            Reporter = x.Reporter.Name,
            Assignee = x.Assignee.Name,
            Tester = x.Tester.Name,
            ReOpenCount = x.ReOpenCount,
            Status = x.Status.Name,
            StatusGroup = x.Status.GroupId.ToString<StatusGroup>(),
            CreatedBy = x.CreatedBy.Name,
            CreatedOn = x.CreatedOn
        }).ToListAsync();

        return data;
    }

    #endregion

    #region Helpers

    private async Task<Status?> ResolveMergeStatusAsync(int? configuredId, string defaultName)
    {
        if (configuredId > 0)
        {
            var configured = await _statusService.GetByIdAsync(configuredId.Value);
            if (configured != null && configured.Active && !configured.Deleted)
                return configured;
        }

        var byName = await _statusService.GetByNameAsync(defaultName);
        return byName != null && byName.Active && !byName.Deleted ? byName : null;
    }

    private async Task<string?> ValidateClosingOnEditAsync(Backlog entity, IList<FieldChange> changes)
    {
        var statusChange = changes.FirstOrDefault(c => c.Name == nameof(Backlog.StatusId));
        if (statusChange == null || statusChange.Value.ToInt() <= 0 || statusChange.Value.ToInt() == entity.StatusId)
            return null;

        var newStatus = await _statusService.GetByIdAsync(statusChange.Value.ToInt());
        if (newStatus == null)
            return null;

        string? ChangedOr(string name, string? current) =>
            changes.FirstOrDefault(c => c.Name == name) is { } c ? c.Value : current;

        var typeChange = changes.FirstOrDefault(c => c.Name == nameof(Backlog.TaskTypeId));
        var probe = new Backlog
        {
            Id = entity.Id,
            TaskTypeId = entity.TaskTypeId,
            TaskType = entity.TaskType,
            MergeRequestId = entity.MergeRequestId,
            RootCauseAnalysis = ChangedOr(nameof(Backlog.RootCauseAnalysis), entity.RootCauseAnalysis),
            ResolutionDetails = ChangedOr(nameof(Backlog.ResolutionDetails), entity.ResolutionDetails)
        };

        var closing = await _closingRequirementService.EvaluateAsync(probe, newStatus, typeChange?.Value.ToInt());
        if (!closing.AnyMissing)
            return null;

        var missing = new List<string>();
        if (closing.RootCauseMissing) missing.Add("root cause");
        if (closing.ResolutionMissing) missing.Add("resolution");
        if (closing.MrMissing) missing.Add("merge request");

        return $"'{newStatus.Name}' requires {string.Join(", ", missing)}.";
    }

    private async Task<string?> ValidateProjectScopeOnEditAsync(Backlog entity, IList<FieldChange> changes)
    {
        FieldChange? Find(string name) => changes.FirstOrDefault(c => c.Name == name);

        var projectChange = Find(nameof(Backlog.ProjectId));
        var projectId = projectChange != null ? projectChange.Value.ToInt() : entity.ProjectId;
        var projectChanged = projectChange != null && projectId != entity.ProjectId;
        if (projectId <= 0)
            return null;

        int Final(string name, int? current)
        {
            var change = Find(name);
            return change != null ? change.Value.ToInt() : current ?? 0;
        }

        bool Check(string name) => projectChanged || Find(name) != null;

        var releaseId = Final(nameof(Backlog.ReleaseId), entity.ReleaseId);
        if (Check(nameof(Backlog.ReleaseId)) && releaseId > 0
            && !await _releaseService.IsAvailableForAllProjectsAsync(releaseId, [projectId]))
        {
            var release = await _releaseService.GetByIdAsync(releaseId);
            return $"Release {release?.Name ?? releaseId.ToString()} is not available for the selected project.";
        }

        var assigneeId = Final(nameof(Backlog.AssigneeId), entity.AssigneeId);
        if (Check(nameof(Backlog.AssigneeId)) && assigneeId > 0
            && !(await _userService.GetAllActiveByProjectAsync(projectId, Constant.DeveloperRoleName)).Any(x => x.Id == assigneeId))
            return "The selected developer is not an active developer on the selected project.";

        var testerId = Final(nameof(Backlog.TesterId), entity.TesterId);
        if (Check(nameof(Backlog.TesterId)) && testerId > 0
            && !(await _userService.GetAllActiveByProjectAsync(projectId, Constant.QualityAssuranceRoleName)).Any(x => x.Id == testerId))
            return "The selected tester is not an active tester on the selected project.";

        var statusId = Final(nameof(Backlog.StatusId), entity.StatusId);
        if (Check(nameof(Backlog.StatusId)) && statusId > 0
            && !(await _statusService.GetAllActiveByProjectAsync(projectId)).Any(x => x.Id == statusId))
            return "The selected status is not available for the selected project.";

        return null;
    }

    private static BacklogComment BuildChangeComment(int taskId, int userId, string property, object? oldValue, object? newValue)
    {
        var label = CommonHelper.Beautify(property);
        var comment = new BacklogComment
        {
            BacklogId = taskId,
            CreatedById = userId,
            CreatedOn = DateTime.UtcNow,
            SystemComment = true
        };

        if (!BacklogTextFields.IsTextField(property))
        {
            comment.Comment = $"{label}: {oldValue} → {newValue}";
            return comment;
        }

        var oldText = oldValue?.ToString();
        var newText = newValue?.ToString();
        var action = string.IsNullOrWhiteSpace(oldText) ? "added"
            : string.IsNullOrWhiteSpace(newText) ? "cleared"
            : "updated";

        comment.Comment = $"{label} {action}";
        comment.FieldName = property;
        comment.OldValue = oldText;
        comment.NewValue = newText;
        return comment;
    }

    private string CleanPropertyName(string property)
    {
        return Regex.Replace(property, "Id$", ".Name", RegexOptions.IgnoreCase);
    }

    public async Task<bool> CanEditTaskAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        var loggedUser = await _workContext.GetCurrentUserAsync();
        return await CanEditTaskAsync(entity, loggedUser.Id);
    }

    public async Task<bool> CanViewTaskAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null || entity.Deleted)
            return false;

        var loggedUser = await _workContext.GetCurrentUserAsync();
        var mapping = await _userService.GetProjectMapping(loggedUser.Id, entity.ProjectId);
        if (mapping == null)
            return false;

        return mapping.CanViewOthersTask || entity.AssigneeId == loggedUser.Id || entity.TesterId == loggedUser.Id;
    }

    private async Task<bool> CanEditTaskAsync(Backlog entity, int userId)
    {
        if (entity.AssigneeId == userId)
            return true;

        var mapping = await _userService.GetProjectMapping(userId, entity.ProjectId);
        return mapping != null && mapping.CanEditOthersTask;
    }

    private async Task<bool> CanChangeProjectAsync(Backlog entity, int userId)
    {
        if (entity.Project != null &&
            (entity.Project.ProjectManagerId == userId || entity.Project.TeamLeadId == userId))
            return true;

        var mapping = await _userService.GetProjectMapping(userId, entity.ProjectId);
        return mapping != null && mapping.CanChangeProject;
    }

    public async Task<bool> CanDeleteTaskAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        var loggedUser = await _workContext.GetCurrentUserAsync();
        var mapping = await _userService.GetProjectMapping(loggedUser.Id, entity.ProjectId);

        return mapping != null && mapping.CanDeleteTask;
    }

    private static IQueryable<Backlog> ApplyBacklogSort(IQueryable<Backlog> query, string sortColumn, string sortDirection)
    {
        var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        switch ((sortColumn ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "release":
                return SortByText(query, x => x.Release.Name, desc);
            case "project":
                return SortByText(query, x => x.Project.Name, desc);
            case "module":
                return SortByText(query, x => x.Module.Name, desc);
            case "submodule":
                return SortByText(query, x => x.SubModule.Name, desc);
            case "reporter":
                return SortByText(query, x => x.Reporter.Name, desc);
            case "assignee":
                return SortByText(query, x => x.Assignee == null ? null
                    : (x.Assignee.FriendlyName == null || x.Assignee.FriendlyName.Trim() == ""
                        ? x.Assignee.FirstName + " " + x.Assignee.LastName
                        : x.Assignee.FriendlyName), desc);
            case "tester":
                return SortByText(query, x => x.Tester == null ? null
                    : (x.Tester.FriendlyName == null || x.Tester.FriendlyName.Trim() == ""
                        ? x.Tester.FirstName + " " + x.Tester.LastName
                        : x.Tester.FriendlyName), desc);
            case "createdby":
                return SortByText(query, x => x.CreatedBy.FriendlyName == null || x.CreatedBy.FriendlyName.Trim() == ""
                    ? x.CreatedBy.FirstName + " " + x.CreatedBy.LastName
                    : x.CreatedBy.FriendlyName, desc);
            case "agedays":
                break;
            default:
                return query.ApplySafeSort(sortColumn, sortDirection);
        }

        int[] openGroups = [StatusGroup.New.ToInt(), StatusGroup.InProgress.ToInt(), StatusGroup.ReOpened.ToInt()];
        var oldestFirst = desc;
        var openFirst = query.OrderBy(x => openGroups.Contains(x.Status.GroupId) ? 0 : 1);

        return oldestFirst
            ? openFirst.ThenBy(x => x.CreatedOn).ThenBy(x => x.Id)
            : openFirst.ThenByDescending(x => x.CreatedOn).ThenByDescending(x => x.Id);
    }

    private static IQueryable<Backlog> SortByText(IQueryable<Backlog> query, Expression<Func<Backlog, string?>> key, bool desc)
    {
        var isEmpty = Expression.Lambda<Func<Backlog, int>>(
            Expression.Condition(
                Expression.Equal(key.Body, Expression.Constant(null, typeof(string))),
                Expression.Constant(1),
                Expression.Constant(0)),
            key.Parameters);

        var emptyLast = query.OrderBy(isEmpty);
        return desc
            ? emptyLast.ThenByDescending(key).ThenByDescending(x => x.Id)
            : emptyLast.ThenBy(key).ThenBy(x => x.Id);
    }

    private IQueryable<Backlog> ApplyAgeFilters(IQueryable<Backlog> query, int? statusDaysMin, int? openDaysMin)
    {
        var hasStatusDays = statusDaysMin is > 0;
        var hasOpenDays = openDaysMin is > 0;
        if (!hasStatusDays && !hasOpenDays)
            return query;

        int[] openGroups = [StatusGroup.New.ToInt(), StatusGroup.InProgress.ToInt(), StatusGroup.ReOpened.ToInt()];
        var logs = _backlogStatusLogRepository.Table;

        query = query.Where(x => openGroups.Contains(x.Status.GroupId));

        if (hasStatusDays)
        {
            var cutoffLocal = DateTime.Now.AddDays(-statusDaysMin!.Value);
            var cutoffUtc = DateTime.UtcNow.AddDays(-statusDaysMin.Value);
            query = query.Where(x =>
                (logs.Any(l => l.BacklogId == x.Id) && logs.Where(l => l.BacklogId == x.Id).Max(l => l.CreatedOn) <= cutoffLocal)
                || (!logs.Any(l => l.BacklogId == x.Id) && x.CreatedOn <= cutoffUtc));
        }

        if (hasOpenDays)
        {
            var cutoffLocal = DateTime.Now.AddDays(-openDaysMin!.Value);
            var cutoffUtc = DateTime.UtcNow.AddDays(-openDaysMin.Value);
            query = query.Where(x =>
                (logs.Any(l => l.BacklogId == x.Id) && logs.Where(l => l.BacklogId == x.Id).Min(l => l.CreatedOn) <= cutoffLocal)
                || (!logs.Any(l => l.BacklogId == x.Id) && x.CreatedOn <= cutoffUtc));
        }

        return query;
    }

    private IQueryable<Backlog> ApplyFilters(IQueryable<Backlog> query,
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
        int? dueDateType,
        DateOnly? dueDate)
    {
        if (createdby.Any())
        {
            query = query.Where(x => createdby.Contains(x.CreatedById));
        }

        if (module.Any())
        {
            query = query.Where(x => module.Contains(x.ModuleId ?? 0));
        }

        if (subModule.Any())
        {
            query = query.Where(x => subModule.Contains(x.SubModuleId ?? 0));
        }

        if (taskType.Any())
        {
            query = query.Where(x => taskType.Contains(x.TaskTypeId));
        }

        if (severity.Any())
        {
            query = query.Where(x => severity.Contains(x.SeverityId));
        }

        if (reporter.Any())
        {
            query = query.Where(x => reporter.Contains(x.ReporterId ?? 0));
        }

        if (assignee.Any())
        {
            query = query.Where(x => assignee.Contains(x.AssigneeId ?? 0));
        }

        if (tester.Any())
        {
            query = query.Where(x => tester.Contains(x.TesterId ?? 0));
        }

        if (status.Any())
        {
            query = query.Where(x => status.Contains(x.StatusId));
        }

        if (statusGroup.Any())
        {
            query = query.Where(x => statusGroup.Contains(x.Status.GroupId));
        }

        if (release.Any())
        {
            query = query.Where(x => release.Contains(x.ReleaseId ?? 0));
        }

        if (dueDate != null && dueDate > DateOnly.MinValue)
        {
            switch (dueDateType)
            {
                case 1:
                    query = query.Where(x => x.DueDate <= dueDate);
                    break;
                case 2:
                    query = query.Where(x => x.DueDate == dueDate);
                    break;
                case 3:
                    query = query.Where(x => x.DueDate > dueDate);
                    break;
            }
        }

        return query;
    }

    private async Task<object?> ResolveFkNameAsync(string property, object value)
    {
        #region Resolve an FK id to its display name (nav props don't reload after an FK change)

        var id = value.ToInt();
        if (id <= 0)
            return property.EndsWith("Id", StringComparison.Ordinal) ? "N/A" : null;

        return property switch
        {
            nameof(Backlog.StatusId) => (await _statusService.GetByIdAsync(id))?.Name,
            nameof(Backlog.TaskTypeId) => (await _taskTypeService.GetByIdAsync(id))?.Name,
            nameof(Backlog.SeverityId) => (await _severityService.GetByIdAsync(id))?.Name,
            nameof(Backlog.ModuleId) => (await _moduleService.GetByIdAsync(id))?.Name,
            nameof(Backlog.SubModuleId) => (await _subModuleService.GetByIdAsync(id))?.Name,
            nameof(Backlog.AssigneeId) => (await _userService.GetByIdAsync(id))?.Name,
            nameof(Backlog.TesterId) => (await _userService.GetByIdAsync(id))?.Name,
            nameof(Backlog.ReporterId) => (await _repoterService.GetByIdAsync(id))?.Name,
            nameof(Backlog.ReleaseId) => (await _releaseService.GetByIdAsync(id))?.Name,
            _ => null
        };

        #endregion
    }

    public TEntity UpdateEntityProperty<TEntity>(TEntity entity, string propertyName, object newValue) where TEntity : class
    {
        var entityType = typeof(TEntity);

        var property = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property != null)
        {
            var propertyType = property.PropertyType;
            if (newValue != null && newValue.ToString() == "-1")
                newValue = null;

            object convertedValue;

            if (propertyType == typeof(DateOnly) ||
            (propertyType.IsGenericType &&
             propertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
             propertyType.GetGenericArguments()[0] == typeof(DateOnly)) && newValue != null)
            {
                convertedValue = DateOnly.FromDateTime(Convert.ToDateTime(newValue));
            }
            else
            {
                var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                convertedValue = newValue == null ? null : Convert.ChangeType(newValue, targetType);
            }

            property.SetValue(entity, convertedValue);

            return entity;
        }

        throw new ArgumentException($"Property '{propertyName}' not found on type '{entityType.Name}'.");
    }

    private async Task AddToNotification(ChatEvent chatEvent, Backlog entity,
        List<ChatTaskUpdateDto> notifications, string message)
    {
        var creator = entity.CreatedBy;
        var assignee = entity.Assignee;
        var tester = entity.Tester;
        var loggedUser = await _workContext.GetCurrentUserAsync();

        if ((creator != null && creator.Id == loggedUser.Id) ||
            (assignee != null && assignee.Id == loggedUser.Id) ||
            (tester != null && tester.Id == loggedUser.Id))
            return;

        if (creator != null)
        {
            if (!notifications.Any(x => x.Email == creator.Email && x.Message == message && x.TaskId == entity.Id))
            {
                notifications.Add(new()
                {
                    Name = creator.Name,
                    Email = creator.Email,
                    TaskId = entity.Id,
                    TaskTitle = entity.Title,
                    Message = message,
                    ProjectId = entity.ProjectId,
                    Event = chatEvent
                });
            }
        }
        if (assignee != null && assignee.Id != creator?.Id)
        {
            if (!notifications.Any(x => x.Email == assignee.Email && x.Message == message && x.TaskId == entity.Id))
            {
                notifications.Add(new()
                {
                    Name = assignee.Name,
                    Email = assignee.Email,
                    TaskId = entity.Id,
                    TaskTitle = entity.Title,
                    Message = message,
                    ProjectId = entity.ProjectId,
                    Event = chatEvent
                });
            }
        }
        if (tester != null && tester.Id != creator?.Id && tester.Id != assignee?.Id)
        {
            if (!notifications.Any(x => x.Email == tester.Email && x.Message == message && x.TaskId == entity.Id))
            {
                notifications.Add(new()
                {
                    Name = tester.Name,
                    Email = tester.Email,
                    TaskId = entity.Id,
                    TaskTitle = entity.Title,
                    Message = message,
                    ProjectId = entity.ProjectId,
                    Event = chatEvent
                });
            }
        }
    }

    #endregion
}