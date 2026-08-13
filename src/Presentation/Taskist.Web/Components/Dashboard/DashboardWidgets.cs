using Microsoft.AspNetCore.Mvc;
using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;
using Taskist.Service.WorkItems;
using Taskist.Web.Models.Dashboard;

namespace Taskist.Web.Components.Dashboard;

/// <summary>
/// Builds the dashboard view model from real backlog data (no fake/demo data).
/// </summary>
public class DashboardWidgets : ViewComponent
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IBacklogItemService _backlogService;

    #endregion

    #region Ctor

    public DashboardWidgets(IWorkContext workContext, IBacklogItemService backlogService)
    {
        _workContext = workContext;
        _backlogService = backlogService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(int projectId = 0, int days = 0)
    {
        var user = await _workContext.GetCurrentUserAsync();
        var allTasks = await _backlogService.GetAccessibleTasksAsync(user.Id);
        var activity = await _backlogService.GetRecentActivityAsync(user.Id, 8);

        var today = DateOnly.FromDateTime(DateTime.Now);
        var soon = today.AddDays(3);

        bool IsClosed(Backlog t) =>
            t.Status != null && t.Status.GroupId == (int)StatusGroupEnum.Resolved;

        // ---- filter options + applied filters ----
        // project choices are derived from what the user can actually see (no extra query, permission-safe)
        var projectOptions = allTasks
            .Where(t => t.Project != null)
            .GroupBy(t => t.ProjectId)
            .Select(g => new DashboardProjectOption { Id = g.Key, Name = g.First().Project.Name })
            .OrderBy(p => p.Name)
            .ToList();

        var tasks = allTasks.AsEnumerable();
        if (projectId > 0)
            tasks = tasks.Where(t => t.ProjectId == projectId);
        if (days > 0)
        {
            var since = DateTime.Now.Date.AddDays(-days);
            tasks = tasks.Where(t => t.CreatedOn >= since);
        }
        var filtered = tasks.ToList();
        tasks = filtered; // materialize once; downstream widgets enumerate the same list

        var model = new DashboardModel
        {
            SelectedProjectId = projectId,
            Days = days,
            ProjectOptions = projectOptions,
            TotalTasks = filtered.Count,
            MyTasks = filtered.Count(t => t.AssigneeId == user.Id),
            ClosedTasks = filtered.Count(IsClosed),
            OpenTasks = filtered.Count(t => !IsClosed(t)),
            OverdueTasks = filtered.Count(t => t.DueDate.HasValue && t.DueDate.Value < today && !IsClosed(t)),
            DueSoonTasks = filtered.Count(t => t.DueDate.HasValue && t.DueDate.Value >= today && t.DueDate.Value <= soon && !IsClosed(t))
        };

        // ---- due-date breakdown (open tasks only) ----
        var tomorrow = today.AddDays(1);
        var next7 = today.AddDays(7);
        var openTasks = tasks.Where(t => !IsClosed(t)).ToList();

        model.DueOverdue = openTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value < today);
        model.DueToday = openTasks.Count(t => t.DueDate == today);
        model.DueTomorrow = openTasks.Count(t => t.DueDate == tomorrow);
        model.DueNext7 = openTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value > tomorrow && t.DueDate.Value <= next7);
        model.DueNone = openTasks.Count(t => !t.DueDate.HasValue);

        // ---- distributions ----
        model.StatusDistribution = tasks
            .Where(t => t.Status != null)
            .GroupBy(t => new { t.Status.Name, t.Status.BackgroundColor })
            .Select(g => new DashboardSlice { Label = g.Key.Name, Count = g.Count(), Color = g.Key.BackgroundColor ?? "#64748b" })
            .OrderByDescending(s => s.Count)
            .ToList();

        model.SeverityDistribution = tasks
            .Where(t => t.Severity != null)
            .GroupBy(t => new { t.Severity.Name, t.Severity.BackgroundColor })
            .Select(g => new DashboardSlice { Label = g.Key.Name, Count = g.Count(), Color = g.Key.BackgroundColor ?? "#64748b" })
            .OrderByDescending(s => s.Count)
            .ToList();

        model.TaskTypeDistribution = tasks
            .Where(t => t.TaskType != null)
            .GroupBy(t => new { t.TaskType.Name, t.TaskType.BackgroundColor })
            .Select(g => new DashboardSlice { Label = g.Key.Name, Count = g.Count(), Color = g.Key.BackgroundColor ?? "#64748b" })
            .OrderByDescending(s => s.Count)
            .ToList();

        // ---- my open tasks (actionable) ----
        model.MyOpenTasks = tasks
            .Where(t => t.AssigneeId == user.Id && !IsClosed(t))
            .OrderBy(t => t.DueDate ?? DateOnly.MaxValue)
            .Take(6)
            .Select(t => new DashboardTask
            {
                Id = t.Id,
                Title = t.Title,
                StatusName = t.Status?.Name,
                StatusColor = t.Status?.BackgroundColor ?? "#64748b",
                SeverityName = t.Severity?.Name,
                SeverityColor = t.Severity?.BackgroundColor ?? "#64748b",
                DueDate = t.DueDate,
                IsOverdue = t.DueDate.HasValue && t.DueDate.Value < today
            })
            .ToList();

        // ---- recent activity feed ----
        model.RecentActivity = activity.Select(a => new DashboardActivity
        {
            BacklogId = a.BacklogId,
            UserId = a.CreatedById,
            UserName = a.CreatedBy?.Name,
            Text = a.Comment,
            When = a.CreatedOn,
            IsSystem = a.SystemComment
        }).ToList();

        return View(model);
    }

    #endregion
}
