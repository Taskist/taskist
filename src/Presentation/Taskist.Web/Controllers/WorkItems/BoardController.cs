using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.WorkItems;

[CheckPermission(nameof(PermissionProvider.ManageBoard))]
public class BoardController : BaseController
{
    #region Fields

    protected readonly IBacklogService _backlogItemService;
    protected readonly IStatusService _statusService;
    protected readonly ITaskTypeService _taskTypeService;
    protected readonly ISeverityService _severityService;
    protected readonly IUserService _userService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public BoardController(IBacklogService backlogItemService,
        IStatusService statusService,
        ITaskTypeService taskTypeService,
        ISeverityService severityService,
        IUserService userService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService)
    {
        _backlogItemService = backlogItemService;
        _statusService = statusService;
        _taskTypeService = taskTypeService;
        _severityService = severityService;
        _userService = userService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
    }

    #endregion

    #region Constants

    private const int PageSize = 20;

    #endregion

    #region Actions

    public async Task<IActionResult> Index(int project)
    {
        var projectId = await ResolveProjectIdAsync(project);

        return View(await BuildBoardAsync(projectId));
    }

    public async Task<IActionResult> Cards(int project, int statusId, int skip, int take)
    {
        var projectId = await ResolveProjectIdAsync(project);
        if (projectId <= 0 || statusId <= 0)
            return PartialView("_BoardCards", new List<BoardCardModel>());

        if (take <= 0)
            take = PageSize;

        var pageIndex = take > 0 ? skip / take : 0;
        var canViewOthers = await CanViewOthersAsync(projectId);
        var paged = await _backlogItemService.GetBoardCardsAsync(projectId, statusId, pageIndex, take, UserId, canViewOthers);

        var status = (await _statusService.GetAllActiveByProjectAsync(projectId)).FirstOrDefault(s => s.Id == statusId);
        var accent = string.IsNullOrWhiteSpace(status?.BackgroundColor) ? "#6f78e6" : status.BackgroundColor;
        var maps = await LoadCardMapsAsync();

        return PartialView("_BoardCards", paged.Select(t => ToCard(t, accent, maps)).ToList());
    }

    public async Task<IActionResult> Columns(int project)
    {
        var projectId = await ResolveProjectIdAsync(project);
        var statuses = (await _statusService.GetAllActiveByProjectAsync(projectId))
            .OrderBy(s => StatusGroupOrder(s.GroupId))
            .ThenBy(s => s.Name)
            .ToList();

        var selected = await GetSelectedStatusIdsAsync(projectId);
        var hasPreference = selected.Count > 0;

        return PartialView(new BoardColumnsModel
        {
            ProjectId = projectId,
            Options = statuses.Select(s => new BoardColumnOptionModel
            {
                StatusId = s.Id,
                Name = s.Name,
                Selected = hasPreference ? selected.Contains(s.Id) : s.GroupId != (int)StatusGroup.Archived
            }).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveColumns(int project, int[] statusIds, bool reset)
    {
        var projectId = await ResolveProjectIdAsync(project);

        var value = reset || statusIds == null || statusIds.Length == 0
            ? string.Empty
            : string.Join(",", statusIds.Distinct());

        // Use a plain (non-proxy) User so the attribute key group matches the read side ("User").
        await _genericAttributeService.SaveAttributeAsync(new User { Id = UserId }, BoardColumnsKey(projectId), value);

        return Json(new JsonResponseModel
        {
            Status = HttpStatusCode.Success,
            Message = await _localizationService.GetResourceAsync("Message.SaveSuccess")
        });
    }

    public async Task<IActionResult> Timeline(int project)
    {
        var projectId = await ResolveProjectIdAsync(project);
        var accessible = await _userService.GetAllAccessibleProjects(UserId);

        return View(new TimelineModel
        {
            ProjectId = projectId,
            Projects = accessible.OrderBy(p => p.Name)
                .Select(p => new SelectListItem(p.Name, p.Id.ToString(), p.Id == projectId))
                .ToList()
        });
    }

    public async Task<IActionResult> TimelineData(int project, string start, string end)
    {
        var projectId = await ResolveProjectIdAsync(project);
        if (projectId <= 0)
            return Json(Array.Empty<object>());

        var today = DateOnly.FromDateTime(DateTime.Now);
        var from = ParseDate(start) ?? today.AddMonths(-1);
        var to = ParseDate(end) ?? today.AddMonths(2);

        var editUrl = Url.Action("Edit", "Backlog");
        var canViewOthers = await CanViewOthersAsync(projectId);
        var tasks = await _backlogItemService.GetTimelineTasksAsync(projectId, from, to, UserId, canViewOthers);
        var maps = await LoadCardMapsAsync();
        var statusColorById = (await _statusService.GetAllActiveByProjectAsync(projectId))
            .ToDictionary(s => s.Id, s => s.BackgroundColor);

        var events = tasks.Select(t =>
        {
            maps.TypeById.TryGetValue(t.TaskTypeId, out var type);
            maps.SeverityById.TryGetValue(t.SeverityId, out var severity);
            var due = t.DueDate;

            return new
            {
                id = t.Id,
                title = t.Title,
                start = (t.PlannedStartDate ?? due ?? DateOnly.FromDateTime(t.CreatedOn)).ToString("yyyy-MM-dd"),
                end = due.HasValue ? due.Value.AddDays(1).ToString("yyyy-MM-dd") : null,
                color = statusColorById.TryGetValue(t.StatusId, out var c) && !string.IsNullOrWhiteSpace(c) ? c : "#6f78e6",
                url = $"{editUrl}/{t.Id}",
                extendedProps = new
                {
                    due = due?.ToString("yyyy-MM-dd"),
                    overdue = due.HasValue && due.Value < today,
                    typeName = type?.Name,
                    typeIcon = type?.IconClass,
                    severityName = severity?.Name,
                    severityColor = string.IsNullOrWhiteSpace(severity?.BackgroundColor) ? "#adb5bd" : severity.BackgroundColor,
                    assignee = t.AssigneeId.HasValue && maps.UserById.TryGetValue(t.AssigneeId.Value, out var n) ? n : null
                }
            };
        });

        return Json(events);
    }

    private static DateOnly? ParseDate(string value)
    {
        return DateTime.TryParse(value, out var dt) ? DateOnly.FromDateTime(dt) : null;
    }

    #endregion

    #region Utilities

    private async Task<int> ResolveProjectIdAsync(int projectId)
    {
        var accessible = await _userService.GetAllAccessibleProjects(UserId);
        if (projectId > 0 && accessible.Any(p => p.Id == projectId))
            return projectId;

        return accessible.FirstOrDefault()?.Id ?? 0;
    }

    // Mirrors the backlog grid: a user only sees others' tasks in projects where they have that right;
    // otherwise they see only tasks where they are the assignee or tester.
    private async Task<bool> CanViewOthersAsync(int projectId)
    {
        var viewAll = await _userService.GetAllAccessibleProjects(UserId, fetchOnlyCanViewOthersTask: true);
        return viewAll.Any(p => p.Id == projectId);
    }

    private async Task<BoardModel> BuildBoardAsync(int projectId)
    {
        var accessible = await _userService.GetAllAccessibleProjects(UserId);

        var model = new BoardModel
        {
            ProjectId = projectId,
            Projects = accessible.OrderBy(p => p.Name)
                .Select(p => new SelectListItem(p.Name, p.Id.ToString(), p.Id == projectId))
                .ToList()
        };

        if (projectId <= 0)
            return model;

        var statuses = (await _statusService.GetAllActiveByProjectAsync(projectId))
            .OrderBy(s => StatusGroupOrder(s.GroupId))
            .ThenBy(s => s.Name)
            .ToList();

        var selected = await GetSelectedStatusIdsAsync(projectId);
        statuses = selected.Count > 0
            ? statuses.Where(s => selected.Contains(s.Id)).ToList()
            : statuses.Where(s => s.GroupId != (int)StatusGroup.Archived).ToList();

        var maps = await LoadCardMapsAsync();
        var canViewOthers = await CanViewOthersAsync(projectId);

        var columns = new List<BoardColumnModel>();
        foreach (var s in statuses)
        {
            var paged = await _backlogItemService.GetBoardCardsAsync(projectId, s.Id, 0, PageSize, UserId, canViewOthers);
            var accent = string.IsNullOrWhiteSpace(s.BackgroundColor) ? "#6f78e6" : s.BackgroundColor;

            columns.Add(new BoardColumnModel
            {
                StatusId = s.Id,
                Name = s.Name,
                TextColor = s.TextColor,
                BackgroundColor = s.BackgroundColor,
                IconClass = s.IconClass,
                Total = paged.TotalCount,
                Loaded = paged.Count,
                Cards = paged.Select(t => ToCard(t, accent, maps)).ToList()
            });
        }

        model.Columns = columns;
        return model;
    }

    private async Task<CardMaps> LoadCardMapsAsync()
    {
        return new CardMaps
        {
            TypeById = (await _taskTypeService.GetAllAsync(true)).ToDictionary(t => t.Id),
            SeverityById = (await _severityService.GetAllAsync(true)).ToDictionary(s => s.Id),
            UserById = (await _userService.GetAllAsync()).ToDictionary(e => e.Id, e => e.Name)
        };
    }

    private sealed class CardMaps
    {
        public IDictionary<int, Core.Domain.Masters.TaskType> TypeById { get; init; }

        public IDictionary<int, Core.Domain.Masters.Severity> SeverityById { get; init; }

        public IDictionary<int, string> UserById { get; init; }
    }

    private static BoardCardModel ToCard(Backlog task, string statusColor, CardMaps maps)
    {
        var typeById = maps.TypeById;
        var severityById = maps.SeverityById;
        var userById = maps.UserById;

        typeById.TryGetValue(task.TaskTypeId, out var type);
        severityById.TryGetValue(task.SeverityId, out var severity);

        return new BoardCardModel
        {
            Id = task.Id,
            Title = task.Title,
            TaskTypeId = task.TaskTypeId,
            StatusColor = statusColor,
            TaskTypeName = type?.Name,
            TypeColor = type?.BackgroundColor,
            TypeTextColor = type?.TextColor,
            TypeIcon = type?.IconClass,
            SeverityName = severity?.Name,
            SeverityColor = severity?.BackgroundColor,
            SeverityTextColor = severity?.TextColor,
            SeverityIcon = severity?.IconClass,
            AssigneeName = task.AssigneeId.HasValue && userById.TryGetValue(task.AssigneeId.Value, out var name) ? name : null,
            DueDate = task.DueDate,
            CommentCount = task.CommentCount ?? 0,
            AttachmentCount = task.AttachmentCount ?? 0,
            SubTaskCount = task.SubTaskCount
        };
    }

    private static string BoardColumnsKey(int projectId) => $"BoardColumns_{projectId}";

    private async Task<HashSet<int>> GetSelectedStatusIdsAsync(int projectId)
    {
        var csv = await _genericAttributeService.GetAttributeAsync<User, string>(UserId, BoardColumnsKey(projectId));
        if (string.IsNullOrWhiteSpace(csv))
            return [];

        return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(s => int.TryParse(s, out _))
            .Select(int.Parse)
            .ToHashSet();
    }

    private static int StatusGroupOrder(int groupId)
    {
        return groupId switch
        {
            (int)StatusGroup.New => 1,
            (int)StatusGroup.InProgress => 2,
            (int)StatusGroup.ReOpened => 3,
            (int)StatusGroup.Resolved => 4,
            (int)StatusGroup.Archived => 5,
            _ => 9
        };
    }

    #endregion
}
