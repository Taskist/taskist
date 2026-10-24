using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Reports;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Masters;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Microsoft.EntityFrameworkCore;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Reports;

public partial class ReportService : IReportService
{
    #region Fields

    protected readonly IRepository<Backlog> _taskRepository;
    protected readonly IRepository<BacklogStatusLog> _statusLogRepository;
    protected readonly IUserService _userService;
    protected readonly ISettingService _settingService;
    protected readonly IStatusDurationMetricService _stageMetricService;

    #endregion

    #region Ctor

    public ReportService(IRepository<Backlog> taskRepository,
        IRepository<BacklogStatusLog> statusLogRepository,
        IUserService userService,
        ISettingService settingService,
        IStatusDurationMetricService stageMetricService)
    {
        _taskRepository = taskRepository;
        _statusLogRepository = statusLogRepository;
        _userService = userService;
        _settingService = settingService;
        _stageMetricService = stageMetricService;
    }

    #endregion

    private static readonly int[] OpenGroups = [(int)StatusGroup.New, (int)StatusGroup.InProgress, (int)StatusGroup.ReOpened];
    private static readonly int[] ClosedGroups = [(int)StatusGroup.Resolved, (int)StatusGroup.Archived];

    public async Task<List<TrendPointDto>> GetCreatedVsResolvedAsync(int[] projectIds, int months = 6)
    {
        if (projectIds == null || projectIds.Length == 0)
            return [];

        var today = DateTime.Today;
        var from = new DateTime(today.Year, today.Month, 1).AddMonths(-(months - 1));

        var created = await _taskRepository.GetAllAsync(q => q
            .Where(x => !x.Deleted && projectIds.Contains(x.ProjectId) && x.CreatedOn >= from));

        var resolved = await _statusLogRepository.GetAllAsync(q => q
            .Include(x => x.Status)
            .Where(x => x.CreatedOn >= from
                && x.Status.GroupId == (int)StatusGroup.Resolved
                && projectIds.Contains(x.Backlog.ProjectId)));

        var createdByMonth = created.GroupBy(x => x.CreatedOn.ToString("yyyy-MM")).ToDictionary(g => g.Key, g => g.Count());
        var resolvedByMonth = resolved.GroupBy(x => x.CreatedOn.ToString("yyyy-MM")).ToDictionary(g => g.Key, g => g.Count());

        var result = new List<TrendPointDto>();
        for (var i = 0; i < months; i++)
        {
            var month = from.AddMonths(i);
            var key = month.ToString("yyyy-MM");
            result.Add(new TrendPointDto
            {
                Period = month.ToString("MMM yyyy"),
                Created = createdByMonth.GetValueOrDefault(key),
                Resolved = resolvedByMonth.GetValueOrDefault(key)
            });
        }

        return result;
    }

    public async Task<List<WorkloadDto>> GetAssigneeWorkloadAsync(int[] projectIds)
    {
        if (projectIds == null || projectIds.Length == 0)
            return [];

        var tasks = await _taskRepository.GetAllAsync(q => q
            .Include(x => x.Assignee)
            .Include(x => x.Status)
            .Where(x => !x.Deleted && projectIds.Contains(x.ProjectId)));

        return tasks
            .GroupBy(x => x.Assignee != null ? x.Assignee.Name : "Unassigned")
            .Select(g => new WorkloadDto
            {
                Assignee = g.Key,
                Open = g.Count(x => OpenGroups.Contains(x.Status.GroupId)),
                Closed = g.Count(x => ClosedGroups.Contains(x.Status.GroupId))
            })
            .OrderByDescending(x => x.Open + x.Closed)
            .ToList();
    }

    public async Task<ResourceLoadResultDto> GetResourceLoadAsync(ResourceLoadFilter filter, int currentUserId, int[] accessibleProjectIds)
    {
        var result = new ResourceLoadResultDto();
        if (accessibleProjectIds == null || accessibleProjectIds.Length == 0)
            return result;

        filter ??= new ResourceLoadFilter();

        var projectScope = (filter.ProjectIds != null && filter.ProjectIds.Length > 0)
            ? filter.ProjectIds.Where(accessibleProjectIds.Contains).ToArray()
            : accessibleProjectIds;
        if (projectScope.Length == 0)
            return result;

        var settings = await _settingService.LoadSettingAsync<ResourceLoadSettings>();
        var today = DateOnly.FromDateTime(DateTime.Today);
        var dueSoonEnd = today.AddDays(settings.DueSoonDays);

        var devs = await _userService.GetAllActiveByRoleAsync(Constant.DeveloperRoleName);
        var qas = await _userService.GetAllActiveByRoleAsync(Constant.QualityAssuranceRoleName);
        var devIds = devs.Select(x => x.Id).ToHashSet();
        var qaIds = qas.Select(x => x.Id).ToHashSet();

        IEnumerable<User> pool = filter.Role switch
        {
            "dev" => devs,
            "qa" => qas,
            _ => devs.Concat(qas).GroupBy(x => x.Id).Select(g => g.First())
        };

        var candidates = pool.ToList();

        if (filter.UserIds is { Length: > 0 })
            candidates = candidates.Where(e => filter.UserIds.Contains(e.Id)).ToList();

        var statusGroups = (filter.StatusGroupIds is { Length: > 0 }) ? filter.StatusGroupIds : OpenGroups;

        var tasks = await _taskRepository.GetAllAsync(q => q
            .Include(x => x.Status)
            .Include(x => x.Severity)
            .Include(x => x.Project)
            .Where(x => !x.Deleted
                && projectScope.Contains(x.ProjectId)
                && statusGroups.Contains(x.Status.GroupId)));

        if (filter.SeverityIds is { Length: > 0 })
            tasks = tasks.Where(x => filter.SeverityIds.Contains(x.SeverityId)).ToList();

        tasks = filter.DueWindow switch
        {
            "overdue" => tasks.Where(x => x.DueDate != null && x.DueDate < today).ToList(),
            "dueweek" => tasks.Where(x => x.DueDate != null && x.DueDate >= today && x.DueDate <= dueSoonEnd).ToList(),
            "range" => tasks.Where(x => x.DueDate != null
                && (filter.From == null || x.DueDate >= filter.From)
                && (filter.To == null || x.DueDate <= filter.To)).ToList(),
            _ => tasks
        };

        var rows = new List<ResourceLoadDto>();

        foreach (var emp in candidates)
        {
            IEnumerable<Backlog> mine = filter.LoadBasis switch
            {
                "tester" => tasks.Where(x => x.TesterId == emp.Id),
                "both" => tasks.Where(x => x.AssigneeId == emp.Id || x.TesterId == emp.Id),
                _ => tasks.Where(x => x.AssigneeId == emp.Id)
            };
            var list = mine.ToList();

            var score = filter.Weighted
                ? list.Sum(x => Weight(SeverityBucket(x.Severity?.Name), settings))
                : list.Count;

            rows.Add(new ResourceLoadDto
            {
                UserId = emp.Id,
                Name = emp.Name,
                Role = devIds.Contains(emp.Id) && qaIds.Contains(emp.Id) ? "Dev/QA" : (qaIds.Contains(emp.Id) ? "QA" : "Developer"),
                Projects = string.Join(", ", list.Where(x => x.Project != null).Select(x => x.Project.Name).Distinct().OrderBy(x => x)),
                Open = list.Count,
                Overdue = list.Count(x => x.DueDate != null && x.DueDate < today),
                DueSoon = list.Count(x => x.DueDate != null && x.DueDate >= today && x.DueDate <= dueSoonEnd),
                Reopened = list.Count(x => x.ReOpenCount > 0),
                CriticalHigh = list.Count(x => SeverityBucket(x.Severity?.Name) is "Critical" or "High"),
                Score = score,
                Band = Band(score, settings, filter.BusyThreshold)
            });
        }

        if (filter.IncludeUnassigned && filter.LoadBasis != "tester")
        {
            var un = tasks.Where(x => x.AssigneeId == null).ToList();
            if (un.Count > 0)
            {
                var score = filter.Weighted ? un.Sum(x => Weight(SeverityBucket(x.Severity?.Name), settings)) : un.Count;
                rows.Add(new ResourceLoadDto
                {
                    UserId = 0,
                    Name = "Unassigned",
                    Role = "—",
                    Projects = string.Join(", ", un.Where(x => x.Project != null).Select(x => x.Project.Name).Distinct().OrderBy(x => x)),
                    Open = un.Count,
                    Overdue = un.Count(x => x.DueDate != null && x.DueDate < today),
                    DueSoon = un.Count(x => x.DueDate != null && x.DueDate >= today && x.DueDate <= dueSoonEnd),
                    Reopened = un.Count(x => x.ReOpenCount > 0),
                    CriticalHigh = un.Count(x => SeverityBucket(x.Severity?.Name) is "Critical" or "High"),
                    Score = score,
                    Band = Band(score, settings, filter.BusyThreshold)
                });
            }
        }

        result.Rows = rows.OrderByDescending(x => x.Score).ThenByDescending(x => x.Overdue).ToList();
        result.Free = rows.Count(x => x.Band == "Free");
        result.Light = rows.Count(x => x.Band == "Light");
        result.Balanced = rows.Count(x => x.Band == "Balanced");
        result.Overloaded = rows.Count(x => x.Band == "Overloaded");

        return result;
    }

    private static string SeverityBucket(string? name)
    {
        var n = (name ?? "").ToLowerInvariant();
        if (n.Contains("critical") || n.Contains("blocker")) return "Critical";
        if (n.Contains("high") || n.Contains("major")) return "High";
        if (n.Contains("low") || n.Contains("minor") || n.Contains("trivial")) return "Low";
        return "Medium";
    }

    private static double Weight(string bucket, ResourceLoadSettings s) => bucket switch
    {
        "Critical" => s.CriticalWeight,
        "High" => s.HighWeight,
        "Low" => s.LowWeight,
        _ => s.MediumWeight
    };

    private static string Band(double score, ResourceLoadSettings s, int? busy)
    {
        if (score <= 0) return "Free";
        if (busy.HasValue && busy.Value > 0)
            return score >= busy.Value ? "Overloaded" : (score <= Math.Max(1, busy.Value / 2) ? "Light" : "Balanced");
        if (score <= s.LightMax) return "Light";
        if (score <= s.BalancedMax) return "Balanced";
        return "Overloaded";
    }

    public async Task<List<TimeInStatusDto>> GetTimeInStatusAsync(int[] projectIds)
    {
        if (projectIds == null || projectIds.Length == 0)
            return [];

        var logs = await _statusLogRepository.GetAllAsync(q => q
            .Include(x => x.Status)
            .Where(x => projectIds.Contains(x.Backlog.ProjectId))
            .OrderBy(x => x.BacklogId).ThenBy(x => x.CreatedOn));

        var totals = new Dictionary<string, (double Days, int Count)>();
        for (var i = 0; i < logs.Count - 1; i++)
        {
            var current = logs[i];
            var next = logs[i + 1];
            if (current.BacklogId != next.BacklogId)
                continue;

            var statusName = current.Status?.Name ?? "Unknown";
            var days = (next.CreatedOn - current.CreatedOn).TotalDays;
            var prev = totals.GetValueOrDefault(statusName);
            totals[statusName] = (prev.Days + days, prev.Count + 1);
        }

        return totals
            .Select(kv => new TimeInStatusDto { Status = kv.Key, AvgDays = Math.Round(kv.Value.Days / kv.Value.Count, 1) })
            .OrderByDescending(x => x.AvgDays)
            .ToList();
    }

    public async Task<List<CycleTimeDto>> GetCycleTimeAsync(int[] projectIds)
    {
        if (projectIds == null || projectIds.Length == 0)
            return [];

        var tasks = await _taskRepository.GetAllAsync(q => q
            .Include(x => x.TaskType)
            .Where(x => !x.Deleted && projectIds.Contains(x.ProjectId)));

        var resolved = await _statusLogRepository.GetAllAsync(q => q
            .Include(x => x.Status)
            .Where(x => x.Status.GroupId == (int)StatusGroup.Resolved && projectIds.Contains(x.Backlog.ProjectId)));

        var firstResolved = resolved
            .GroupBy(x => x.BacklogId)
            .ToDictionary(g => g.Key, g => g.Min(x => x.CreatedOn));

        return tasks
            .Where(x => firstResolved.ContainsKey(x.Id))
            .Select(x => new { Type = x.TaskType != null ? x.TaskType.Name : "Unknown", Days = (firstResolved[x.Id] - x.CreatedOn).TotalDays })
            .Where(x => x.Days >= 0)
            .GroupBy(x => x.Type)
            .Select(g => new CycleTimeDto { Category = g.Key, Count = g.Count(), AvgDays = Math.Round(g.Average(x => x.Days), 1) })
            .OrderByDescending(x => x.Count)
            .ToList();
    }

    #region Stage duration

    public async Task<List<StageDurationDto>> GetStageDurationAsync(int[] projectIds, DateTime? from = null, DateTime? to = null, int? taskTypeId = null)
    {
        if (projectIds == null || projectIds.Length == 0)
            return [];

        var metrics = await _stageMetricService.GetAllAsync();
        metrics = metrics
            .Where(m => m.Active && (m.ProjectId == null || projectIds.Contains(m.ProjectId.Value)))
            .ToList();

        if (metrics.Count == 0)
            return [];

        var spansByMetric = await BuildSpansAsync(metrics, projectIds, from, to, taskTypeId);

        return metrics
            .Select(m =>
            {
                var days = spansByMetric.TryGetValue(m.Id, out var list)
                    ? list.Select(x => x.Days).OrderBy(x => x).ToList()
                    : [];

                if (days.Count == 0)
                    return new StageDurationDto
                    {
                        MetricId = m.Id,
                        Metric = m.Name,
                        FromStatus = m.FromStatus?.Name,
                        ToStatus = m.ToStatus?.Name,
                        Count = 0
                    };

                var avg = days.Average();
                var min = days.First();
                var max = days.Last();
                var median = days.Count % 2 == 1
                    ? days[days.Count / 2]
                    : (days[days.Count / 2 - 1] + days[days.Count / 2]) / 2.0;

                return new StageDurationDto
                {
                    MetricId = m.Id,
                    Metric = m.Name,
                    FromStatus = m.FromStatus?.Name,
                    ToStatus = m.ToStatus?.Name,
                    Count = days.Count,
                    AvgDays = Math.Round(avg, 1),
                    MedianDays = Math.Round(median, 1),
                    MinDays = Math.Round(min, 1),
                    MaxDays = Math.Round(max, 1),
                    AvgText = StageDurationCalculator.SmartText(avg),
                    MedianText = StageDurationCalculator.SmartText(median),
                    MinText = StageDurationCalculator.SmartText(min),
                    MaxText = StageDurationCalculator.SmartText(max)
                };
            })
            .OrderBy(x => x.Metric)
            .ToList();
    }

    public async Task<List<StageDurationTaskDto>> GetStageDurationTasksAsync(int metricId, int[] projectIds, DateTime? from = null, DateTime? to = null, int? taskTypeId = null)
    {
        if (projectIds == null || projectIds.Length == 0)
            return [];

        var metric = await _stageMetricService.GetByIdAsync(metricId);
        if (metric == null || !metric.Active)
            return [];

        if (metric.ProjectId != null && !projectIds.Contains(metric.ProjectId.Value))
            return [];

        var spansByMetric = await BuildSpansAsync([metric], projectIds, from, to, taskTypeId);
        if (!spansByMetric.TryGetValue(metric.Id, out var spans))
            return [];

        return spans
            .OrderByDescending(x => x.Days)
            .Select(x => new StageDurationTaskDto
            {
                TaskId = x.TaskId,
                Title = x.Title,
                Project = x.Project,
                TaskType = x.TaskType,
                FromDate = x.From,
                ToDate = x.To,
                Days = Math.Round(x.Days, 1),
                DurationText = StageDurationCalculator.SmartText(x.Days)
            })
            .ToList();
    }

    private async Task<Dictionary<int, List<(int TaskId, string Title, string Project, string TaskType, DateTime From, DateTime To, double Days)>>>
        BuildSpansAsync(IList<StatusDurationMetric> metrics, int[] projectIds, DateTime? from, DateTime? to, int? taskTypeId)
    {
        if (to != null)
            to = to.Value.Date.AddDays(1).AddTicks(-1);

        var tasks = await _taskRepository.GetAllAsync(q => q
            .Include(x => x.Project)
            .Include(x => x.TaskType)
            .Where(x => !x.Deleted
                && projectIds.Contains(x.ProjectId)
                && (taskTypeId == null || x.TaskTypeId == taskTypeId)));

        var taskMap = tasks.ToDictionary(x => x.Id);

        var logs = await _statusLogRepository.GetAllAsync(q => q
            .Where(x => projectIds.Contains(x.Backlog.ProjectId))
            .OrderBy(x => x.BacklogId).ThenBy(x => x.CreatedOn));

        var logsByTask = logs
            .GroupBy(x => x.BacklogId)
            .ToDictionary(g => g.Key, g => g.Select(l => (l.StatusId, l.CreatedOn)).ToList());

        var result = new Dictionary<int, List<(int TaskId, string Title, string Project, string TaskType, DateTime From, DateTime To, double Days)>>();

        foreach (var metric in metrics)
        {
            var list = new List<(int TaskId, string Title, string Project, string TaskType, DateTime From, DateTime To, double Days)>();

            foreach (var kv in logsByTask)
            {
                if (!taskMap.TryGetValue(kv.Key, out var task))
                    continue;

                var span = StageDurationCalculator.FirstPassSpan(kv.Value, metric.FromStatusId, metric.ToStatusId);
                if (span == null)
                    continue;

                if (from != null && span.Value.To < from.Value)
                    continue;
                if (to != null && span.Value.To > to.Value)
                    continue;

                var days = (span.Value.To - span.Value.From).TotalDays;
                if (days < 0)
                    continue;

                list.Add((task.Id, task.Title, task.Project?.Name, task.TaskType?.Name, span.Value.From, span.Value.To, days));
            }

            result[metric.Id] = list;
        }

        return result;
    }

    #endregion
}
