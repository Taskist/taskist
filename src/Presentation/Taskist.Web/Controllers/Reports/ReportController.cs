using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Dtos.Reports;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Reports;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Mapping;
using Taskist.Web.Helpers.Reports;
using Taskist.Web.Models.Datatable;
using Taskist.Web.Models.Reports;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Controllers.Reports;

public class ReportController : BaseController
{
    #region Fields

    protected readonly IBacklogService _backlogService;
    protected readonly IReportService _reportService;
    protected readonly IPermissionService _permissionService;
    protected readonly IUserService _userService;
    protected readonly ISeverityService _severityService;
    protected readonly ITaskTypeService _taskTypeService;
    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IWorkContext _workContext;
    protected readonly ReportExportSettings _reportExportSettings;
    protected readonly MastersMapper _mapper;

    #endregion

    #region Ctor

    public ReportController(IBacklogService backlogService,
        IReportService reportService,
        IPermissionService permissionService,
        IUserService userService,
        ISeverityService severityService,
        ITaskTypeService taskTypeService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        ReportExportSettings reportExportSettings,
        MastersMapper mapper)
    {
        _backlogService = backlogService;
        _reportService = reportService;
        _permissionService = permissionService;
        _userService = userService;
        _severityService = severityService;
        _taskTypeService = taskTypeService;
        _genericAttributeService = genericAttributeService;
        _localizationService = localizationService;
        _workContext = workContext;
        _reportExportSettings = reportExportSettings;
        _mapper = mapper;
    }

    #endregion

    #region Landing

    public IActionResult Index()
    {
        return RedirectToAction("Index", "Home");
    }

    #endregion

    #region Analytics

    private async Task<int[]> AccessibleProjectIdsAsync()
    {
        return (await _userService.GetAllAccessibleProjects(UserId)).Select(x => x.Id).ToArray();
    }

    private async Task<int[]> ScopedProjectIdsAsync(int? projectId)
    {
        var accessible = await AccessibleProjectIdsAsync();
        if (projectId is > 0 && accessible.Contains(projectId.Value))
            return [projectId.Value];

        return accessible;
    }

    [CheckPermission(nameof(PermissionProvider.ManageResourceReport))]
    public async Task<IActionResult> ResourceLoad()
    {
        var model = new ResourceLoadPageModel();

        foreach (var p in await _userService.GetAllAccessibleProjects(UserId))
            model.AvailableProjects.Add(new SelectListItem { Text = p.Name, Value = p.Id.ToString() });

        foreach (var s in await _severityService.GetAllActiveAsync())
            model.AvailableSeverities.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

        var devs = await _userService.GetAllActiveByRoleAsync(Constant.DeveloperRoleName);
        var qas = await _userService.GetAllActiveByRoleAsync(Constant.QualityAssuranceRoleName);
        foreach (var e in devs.Concat(qas).GroupBy(x => x.Id).Select(g => g.First()).OrderBy(x => x.Name))
            model.AvailableUsers.Add(new SelectListItem { Text = e.Name, Value = e.Id.ToString() });

        foreach (var g in new[] { StatusGroup.New, StatusGroup.InProgress, StatusGroup.ReOpened, StatusGroup.Resolved, StatusGroup.Archived })
            model.AvailableStatusGroups.Add(new SelectListItem { Text = g.ToString(), Value = ((int)g).ToString() });

        return View(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManageResourceReport))]
    public async Task<IActionResult> ResourceLoadData([FromQuery] ResourceLoadFilter filter)
        => Json(await _reportService.GetResourceLoadAsync(filter, UserId, await AccessibleProjectIdsAsync()));

    [CheckPermission(nameof(PermissionProvider.ManageResourceReport))]
    public async Task<IActionResult> ExportResourceLoad([FromQuery] ResourceLoadFilter filter)
    {
        var data = (await _reportService.GetResourceLoadAsync(filter, UserId, await AccessibleProjectIdsAsync())).Rows;
        return ExcelFile(ReportExcelBuilder.Build(data,
            [new() { Key = "Name", Title = "Resource" },
             new() { Key = "Role", Title = "Role" },
             new() { Key = "Projects", Title = "Projects" },
             new() { Key = "Open", Title = "Open" },
             new() { Key = "Overdue", Title = "Overdue" },
             new() { Key = "DueSoon", Title = "Due soon" },
             new() { Key = "Reopened", Title = "Reopened" },
             new() { Key = "CriticalHigh", Title = "Critical/High" },
             new() { Key = "Score", Title = "Score" },
             new() { Key = "Band", Title = "Band" }], "ResourceLoad"), "Resource-Load");
    }

    #endregion

    #region Stage duration

    [CheckPermission(nameof(PermissionProvider.ManageStageDuration))]
    public async Task<IActionResult> StageDuration()
    {
        var model = new StageDurationPageModel();

        foreach (var p in await _userService.GetAllAccessibleProjects(UserId))
            model.AvailableProjects.Add(new SelectListItem { Text = p.Name, Value = p.Id.ToString() });

        foreach (var t in await _taskTypeService.GetAllActiveAsync())
            model.AvailableTaskTypes.Add(new SelectListItem { Text = t.Name, Value = t.Id.ToString() });

        return View(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManageStageDuration))]
    public async Task<IActionResult> StageDurationData(int? projectId, DateTime? from, DateTime? to, int? taskTypeId)
        => Json(await _reportService.GetStageDurationAsync(await ScopedProjectIdsAsync(projectId), from, to, taskTypeId));

    [CheckPermission(nameof(PermissionProvider.ManageStageDuration))]
    public async Task<IActionResult> StageDurationTasks(int metricId, int? projectId, DateTime? from, DateTime? to, int? taskTypeId)
        => Json(await _reportService.GetStageDurationTasksAsync(metricId, await ScopedProjectIdsAsync(projectId), from, to, taskTypeId));

    [CheckPermission(nameof(PermissionProvider.ManageStageDuration))]
    public async Task<IActionResult> ExportStageDuration(int? projectId, DateTime? from, DateTime? to, int? taskTypeId)
    {
        var data = await _reportService.GetStageDurationAsync(await ScopedProjectIdsAsync(projectId), from, to, taskTypeId);
        return ExcelFile(ReportExcelBuilder.Build(data,
            [new() { Key = "Metric", Title = "Metric" },
             new() { Key = "FromStatus", Title = "From" },
             new() { Key = "ToStatus", Title = "To" },
             new() { Key = "Count", Title = "Tasks" },
             new() { Key = "AvgDays", Title = "Avg Days" },
             new() { Key = "MedianDays", Title = "Median Days" },
             new() { Key = "MinDays", Title = "Min Days" },
             new() { Key = "MaxDays", Title = "Max Days" }], "StageDuration"), "Stage-Duration");
    }

    [CheckPermission(nameof(PermissionProvider.ManageStageDuration))]
    public async Task<IActionResult> ExportStageDurationTasks(int metricId, int? projectId, DateTime? from, DateTime? to, int? taskTypeId)
    {
        var data = await _reportService.GetStageDurationTasksAsync(metricId, await ScopedProjectIdsAsync(projectId), from, to, taskTypeId);
        return ExcelFile(ReportExcelBuilder.Build(data,
            [new() { Key = "TaskId", Title = "#" },
             new() { Key = "Title", Title = "Title" },
             new() { Key = "Project", Title = "Project" },
             new() { Key = "TaskType", Title = "Type" },
             new() { Key = "FromDate", Title = "From" },
             new() { Key = "ToDate", Title = "To" },
             new() { Key = "DurationText", Title = "Duration" },
             new() { Key = "Days", Title = "Days" }], "StageDurationTasks"), "Stage-Duration-Detail");
    }

    private static IActionResult ExcelFile(byte[] bytes, string name) =>
        new FileContentResult(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = name + ".xlsx" };

    #endregion

    #region Actions

    [CheckPermission(nameof(PermissionProvider.ManagePivotReport))]
    public async Task<IActionResult> TaskExplorer()
    {
        var model = new TaskExplorerPageModel();
        foreach (var p in await _userService.GetAllAccessibleProjects(UserId))
            model.AvailableProjects.Add(new SelectListItem { Text = p.Name, Value = p.Id.ToString() });

        return View(model);
    }

    [CheckPermission(nameof(PermissionProvider.ManagePivotReport))]
    public IActionResult PivotDetails(string? project,
        string? module,
        string? subModule,
        string? severity,
        string? release,
        string? reporter,
        string? assignee,
        string? tester,
        string? taskType,
        string? status)
    {
        var model = new PivotDetailsModel
        {
            ProjectName = project,
            ModuleName = module,
            SubModuleName = subModule,
            SeverityName = severity,
            ReleaseName = release,
            ReporterName = reporter,
            DeveloperName = assignee,
            TesterName = tester,
            TaskTypeName = taskType,
            StatusName = status
        };

        return PartialView(model);
    }

    #endregion

    #region Data

    public async Task<List<BacklogPivotDto>> GetPivotData(int? project)
    {
        var accessible = await AccessibleProjectIdsAsync();
        var projectId = project is > 0 && accessible.Contains(project.Value) ? project.Value : accessible.FirstOrDefault();
        return await _backlogService.GetPivotDataAsync(projectId);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageBacklog))]
    public async Task<IActionResult> DataRead(DataTableRequest request,
       string? projectName,
       string? moduleName,
       string? subModuleName,
       string? severityName,
       string? releaseName,
       string? reporterName,
       string? developerName,
       string? testerName,
       string? taskTypeName,
       string? statusName)
    {
        var projectId = (await AccessibleProjectIdsAsync()).FirstOrDefault();

        var data = await _backlogService.GetPagedListForPivotAsync(UserId,
            projectId,
            projectName,
            moduleName,
            subModuleName,
            severityName,
            releaseName,
            reporterName,
            developerName,
            testerName,
            taskTypeName,
            statusName,
            request.SearchValue,
            request.Start,
            request.Length,
            request.SortColumn,
            request.SortDirection);

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
                SubTaskCount = x.SubTaskCount,
                CreatedBy = x.CreatedBy.Name,
                CreatedOn = x.CreatedOn,
                ModifiedBy = x.ModifiedBy?.Name,
                ModifiedOn = x.ModifiedOn,
                TaskType = _mapper.ToModel(x.TaskType),
                Severity = _mapper.ToModel(x.Severity),
                Status = _mapper.ToModel(x.Status),
                AttachmentCount = x.AttachmentCount == null ? 0 : x.AttachmentCount.ToInt(),
                CommentCount = x.CommentCount == null ? 0 : x.CommentCount.ToInt(),
                MrId = x.MergeRequestId,
                HasClosingComment = !string.IsNullOrEmpty(x.RootCauseAnalysis)
                    || !string.IsNullOrEmpty(x.ResolutionDetails)
                    || (x.MergeRequestId != null && x.MergeRequestId > 0),
            }),
            recordsFiltered = data.TotalCount,
            recordsTotal = data.TotalCount
        });
    }

    #endregion
}
