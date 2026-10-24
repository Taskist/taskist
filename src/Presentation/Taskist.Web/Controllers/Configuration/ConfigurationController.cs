using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Enums;
using Taskist.Core.Security;
using Taskist.Service.Git;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Messages;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Helpers.Reports;
using Taskist.Web.Models.Configuration;

namespace Taskist.Web.Controllers.Configuration;

[CheckPermission(nameof(PermissionProvider.ManageConfiguration))]
public class ConfigurationController : BaseController
{
    protected readonly ISettingService _settingService;
    protected readonly IPermissionService _permissionService;
    protected readonly IUserService _userService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IMessageService _messageService;
    protected readonly IAppLinkBuilder _linkBuilder;
    protected readonly IStatusService _statusService;
    protected readonly IStatusDurationMetricService _stageMetricService;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly KeyRingProtectionInfo _keyRingProtectionInfo;

    public ConfigurationController(ISettingService settingService,
        IPermissionService permissionService,
        IUserService userService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IMessageService messageService,
        IAppLinkBuilder linkBuilder,
        IStatusService statusService,
        IStatusDurationMetricService stageMetricService,
        IGitRepositoryService gitRepositoryService,
        KeyRingProtectionInfo keyRingProtectionInfo)
    {
        _gitRepositoryService = gitRepositoryService;
        _keyRingProtectionInfo = keyRingProtectionInfo;
        _settingService = settingService;
        _permissionService = permissionService;
        _userService = userService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _messageService = messageService;
        _linkBuilder = linkBuilder;
        _statusService = statusService;
        _stageMetricService = stageMetricService;
    }

    public async Task<IActionResult> Index(string tab = "notifications")
    {
        var model = await BuildModelAsync();
        model.ActiveTab = ResolveActiveTab(model, tab);

        return View(model);
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageNotificationSettings))]
    public async Task<IActionResult> SaveNotifications([Bind(Prefix = "Notifications")] NotificationSettingsModel model)
    {
        var settings = await _settingService.LoadSettingAsync<NotificationSettings>();

        settings.EnableDailyDigest = model.EnableDailyDigest;
        settings.EnableOpenTaskDigest = model.EnableOpenTaskDigest;
        settings.EnableProjectStatusDigest = model.EnableProjectStatusDigest;
        settings.SummaryForProjectManager = model.SummaryForProjectManager;
        settings.SummaryForTeamLead = model.SummaryForTeamLead;

        await _settingService.SaveSettingAsync(settings);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "notifications" });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageGeneralSettings))]
    public async Task<IActionResult> SaveGeneral([Bind(Prefix = "General")] GeneralSettingsModel model)
    {
        var settings = await _settingService.LoadSettingAsync<GeneralSettings>();

        settings.CompanyName = model.CompanyName;
        settings.SupportEmail = model.SupportEmail;
        settings.ItemsPerPage = model.ItemsPerPage;

        await _settingService.SaveSettingAsync(settings);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "general" });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageSecuritySettings))]
    public async Task<IActionResult> SaveSecurity([Bind(Prefix = "Security")] SecuritySettingsModel model)
    {
        var settings = await _settingService.LoadSettingAsync<SecuritySettings>();

        settings.PasswordMinLength = model.PasswordMinLength;
        settings.MaxFailedLoginAttempts = model.MaxFailedLoginAttempts;
        settings.SessionTimeoutMinutes = model.SessionTimeoutMinutes;
        settings.PasswordExpiryDays = model.PasswordExpiryDays;
        settings.TokenExpiryMinutes = model.TokenExpiryMinutes > 0 ? model.TokenExpiryMinutes : 60;
        settings.RememberMeDurationDays = model.RememberMeDurationDays > 0 ? model.RememberMeDurationDays : 30;

        await _settingService.SaveSettingAsync(settings);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "security" });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageWorkItemSettings))]
    public async Task<IActionResult> SaveWorkItem([Bind(Prefix = "WorkItem")] WorkItemSettingsModel model)
    {
        var settings = await _settingService.LoadSettingAsync<WorkItemSettings>();

        settings.DefaultDueInDays = model.DefaultDueInDays;
        settings.MaxReopenCount = model.MaxReopenCount;
        settings.AllowSelfAssign = model.AllowSelfAssign;

        var format = string.IsNullOrWhiteSpace(model.ReleaseVersionFormat) ? ReleaseVersionTemplate.DefaultFormat : model.ReleaseVersionFormat.Trim();
        if (!ReleaseVersionTemplate.IsValid(format))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("ProjectModel.ReleaseVersionFormat.InvalidMsg"));
            return RedirectToAction("Index", new { tab = "workitem" });
        }
        settings.ReleaseVersionFormat = format;
        settings.ReleaseFullIncrement = Enum.IsDefined(typeof(ReleaseVersionPart), model.ReleaseFullIncrement)
            ? model.ReleaseFullIncrement
            : (int)ReleaseVersionPart.Minor;

        await _settingService.SaveSettingAsync(settings);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "workitem" });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageIntegrationSettings))]
    public async Task<IActionResult> SaveIntegration([Bind(Prefix = "Integration")] IntegrationSettingsModel model)
    {
        var settings = await _settingService.LoadSettingAsync<IntegrationSettings>();

        settings.AllowBranchCreation = model.AllowBranchCreation;
        settings.BranchTaskKeyPrefix = string.IsNullOrWhiteSpace(model.BranchTaskKeyPrefix) ? null : model.BranchTaskKeyPrefix.Trim().ToUpperInvariant();

        settings.MergedStatusId = model.MergedStatusId > 0 ? model.MergedStatusId : null;
        settings.RejectedStatusId = model.RejectedStatusId > 0 ? model.RejectedStatusId : null;
        settings.OpenedStatusId = model.OpenedStatusId > 0 ? model.OpenedStatusId : null;
        settings.MoveTasksOnMergeRequestOpen = model.MoveTasksOnMergeRequestOpen;
        settings.SharedBranchFallbackLimit = Math.Clamp(model.SharedBranchFallbackLimit, 0, 50);
        settings.AutoAssignTeamLeadReviewer = model.AutoAssignTeamLeadReviewer;

        await _settingService.SaveSettingAsync(settings);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "integration" });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageReportSettings))]
    public async Task<IActionResult> SaveReports([Bind(Prefix = "Reports")] ReportSettingsModel model)
    {
        var normalized = NormalizeLayoutsJson(model.LayoutsJson);
        if (normalized == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("ReportSettings.InvalidJson"));
            return RedirectToAction("Index", new { tab = "reports" });
        }

        var settings = await _settingService.LoadSettingAsync<ReportExportSettings>();
        settings.LayoutsJson = normalized;
        await _settingService.SaveSettingAsync(settings);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "reports" });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageResourceReport))]
    public async Task<IActionResult> SaveResource([Bind(Prefix = "Resource")] ResourceSettingsModel model)
    {
        var settings = await _settingService.LoadSettingAsync<ResourceLoadSettings>();

        settings.LightMax = model.LightMax > 0 ? model.LightMax : 3;
        settings.BalancedMax = model.BalancedMax > settings.LightMax ? model.BalancedMax : settings.LightMax + 5;
        settings.DueSoonDays = model.DueSoonDays > 0 ? model.DueSoonDays : 7;
        settings.CriticalWeight = model.CriticalWeight > 0 ? model.CriticalWeight : 3;
        settings.HighWeight = model.HighWeight > 0 ? model.HighWeight : 2;
        settings.MediumWeight = model.MediumWeight > 0 ? model.MediumWeight : 1.5;
        settings.LowWeight = model.LowWeight > 0 ? model.LowWeight : 1;

        await _settingService.SaveSettingAsync(settings);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "resource" });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageStageDuration))]
    public async Task<IActionResult> SaveStageDuration([Bind(Prefix = "StageDuration")] StageDurationSettingsModel model)
    {
        List<StageDurationRowModel> rows;
        try
        {
            rows = JsonConvert.DeserializeObject<List<StageDurationRowModel>>(model.MetricsJson ?? "[]") ?? [];
        }
        catch
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("StageDuration.InvalidRows"));
            return RedirectToAction("Index", new { tab = "stageduration" });
        }

        var valid = rows
            .Where(r => !string.IsNullOrWhiteSpace(r.Name) && r.FromStatusId > 0 && r.ToStatusId > 0 && r.FromStatusId != r.ToStatusId)
            .Select(r => new StatusDurationMetric
            {
                Id = r.Id,
                Name = r.Name.Trim(),
                FromStatusId = r.FromStatusId,
                ToStatusId = r.ToStatusId,
                ProjectId = r.ProjectId is > 0 ? r.ProjectId : null,
                ShowOnTask = r.ShowOnTask,
                Active = r.Active
            })
            .ToList();

        await _stageMetricService.ReplaceAllAsync(valid);
        await NotifySavedAsync();

        return RedirectToAction("Index", new { tab = "stageduration" });
    }

    private async Task NotifySavedAsync()
    {
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Message.UpdateSuccess"));
    }

    private async Task<ConfigurationModel> BuildModelAsync()
    {
        var model = new ConfigurationModel
        {
            CanNotifications = await _permissionService.AuthorizeAsync(PermissionProvider.ManageNotificationSettings),
            CanGeneral = await _permissionService.AuthorizeAsync(PermissionProvider.ManageGeneralSettings),
            CanSecurity = await _permissionService.AuthorizeAsync(PermissionProvider.ManageSecuritySettings),
            CanWorkItem = await _permissionService.AuthorizeAsync(PermissionProvider.ManageWorkItemSettings),
            CanIntegration = await _permissionService.AuthorizeAsync(PermissionProvider.ManageIntegrationSettings),
            CanReports = await _permissionService.AuthorizeAsync(PermissionProvider.ManageReportSettings),
            CanResource = await _permissionService.AuthorizeAsync(PermissionProvider.ManageResourceReport),
            CanStageDuration = await _permissionService.AuthorizeAsync(PermissionProvider.ManageStageDuration)
        };

        var notifications = await _settingService.LoadSettingAsync<NotificationSettings>();
        model.Notifications = new NotificationSettingsModel
        {
            EnableDailyDigest = notifications.EnableDailyDigest,
            EnableOpenTaskDigest = notifications.EnableOpenTaskDigest,
            EnableProjectStatusDigest = notifications.EnableProjectStatusDigest,
            SummaryForProjectManager = notifications.SummaryForProjectManager,
            SummaryForTeamLead = notifications.SummaryForTeamLead
        };

        var general = await _settingService.LoadSettingAsync<GeneralSettings>();
        model.General = new GeneralSettingsModel
        {
            CompanyName = general.CompanyName,
            SupportEmail = general.SupportEmail,
            ItemsPerPage = general.ItemsPerPage
        };

        var security = await _settingService.LoadSettingAsync<SecuritySettings>();
        model.Security = new SecuritySettingsModel
        {
            PasswordMinLength = security.PasswordMinLength,
            MaxFailedLoginAttempts = security.MaxFailedLoginAttempts,
            SessionTimeoutMinutes = security.SessionTimeoutMinutes,
            PasswordExpiryDays = security.PasswordExpiryDays,
            TokenExpiryMinutes = security.TokenExpiryMinutes,
            RememberMeDurationDays = security.RememberMeDurationDays
        };

        var workItem = await _settingService.LoadSettingAsync<WorkItemSettings>();
        model.WorkItem = new WorkItemSettingsModel
        {
            DefaultDueInDays = workItem.DefaultDueInDays,
            MaxReopenCount = workItem.MaxReopenCount,
            AllowSelfAssign = workItem.AllowSelfAssign,
            ReleaseVersionFormat = workItem.ReleaseVersionFormat,
            ReleaseFullIncrement = workItem.ReleaseFullIncrement
        };
        await PopulateWorkItemAsync(model.WorkItem);

        var integration = await _settingService.LoadSettingAsync<IntegrationSettings>();
        model.Integration = new IntegrationSettingsModel
        {
            GitConnectionCount = (await _gitRepositoryService.GetConnectionsAsync()).Count,
            KeyRingProtected = _keyRingProtectionInfo.ProtectedAtRest,
            AllowBranchCreation = integration.AllowBranchCreation,
            BranchTaskKeyPrefix = integration.BranchTaskKeyPrefix,
            SharedBranchFallbackLimit = integration.SharedBranchFallbackLimit,
            AutoAssignTeamLeadReviewer = integration.AutoAssignTeamLeadReviewer
        };
        await PopulateIntegrationAsync(model.Integration, integration);

        var reportSettings = await _settingService.LoadSettingAsync<ReportExportSettings>();
        model.Reports = BuildReportsModel(reportSettings.LayoutsJson);

        var resource = await _settingService.LoadSettingAsync<ResourceLoadSettings>();
        model.Resource = new ResourceSettingsModel
        {
            LightMax = resource.LightMax,
            BalancedMax = resource.BalancedMax,
            DueSoonDays = resource.DueSoonDays,
            CriticalWeight = resource.CriticalWeight,
            HighWeight = resource.HighWeight,
            MediumWeight = resource.MediumWeight,
            LowWeight = resource.LowWeight
        };

        if (model.CanStageDuration)
            await PopulateStageDurationAsync(model.StageDuration);

        return model;
    }

    private async Task PopulateStageDurationAsync(StageDurationSettingsModel model)
    {
        model.Metrics = (await _stageMetricService.GetAllAsync())
            .Select(m => new StageDurationRowModel
            {
                Id = m.Id,
                Name = m.Name,
                FromStatusId = m.FromStatusId,
                ToStatusId = m.ToStatusId,
                ProjectId = m.ProjectId,
                ShowOnTask = m.ShowOnTask,
                Active = m.Active
            })
            .ToList();

        model.MetricsJson = JsonConvert.SerializeObject(model.Metrics);

        foreach (var status in await _statusService.GetAllActiveAsync())
            model.AvailableStatuses.Add(new SelectListItem { Text = status.Name, Value = status.Id.ToString() });

        foreach (var project in await _userService.GetAllAccessibleProjects(UserId))
            model.AvailableProjects.Add(new SelectListItem { Text = project.Name, Value = project.Id.ToString() });
    }

    private static ReportSettingsModel BuildReportsModel(string layoutsJson)
    {
        var model = new ReportSettingsModel { LayoutsJson = NormalizeLayoutsJson(layoutsJson) ?? "{}" };

        foreach (var key in Enum.GetValues<ReportExportKey>())
        {
            model.Reports.Add(new ReportLayoutModel
            {
                Key = key.ToString(),
                Name = ReportColumnRegistry.GetName(key),
                EditTitles = true,
                Columns = ReportLayoutResolver.GetEditableLayout(layoutsJson, key)
                    .Select(c => new ReportColumnItemModel { Key = c.Key, Title = c.Title, Included = c.Included })
                    .ToList()
            });
        }

        return model;
    }

    private static string NormalizeLayoutsJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            json = "{}";

        try
        {
            JsonConvert.DeserializeObject<Dictionary<string, List<ReportColumn>>>(json);
        }
        catch
        {
            return null;
        }

        var normalized = new Dictionary<string, List<ReportColumn>>();
        foreach (var key in Enum.GetValues<ReportExportKey>())
            normalized[key.ToString()] = ReportLayoutResolver.GetEditableLayout(json, key);

        return JsonConvert.SerializeObject(normalized);
    }

    private async Task PopulateIntegrationAsync(IntegrationSettingsModel model, IntegrationSettings settings)
    {
        var statuses = (await _statusService.GetAllActiveAsync()).OrderBy(x => x.ProjectId != null).ThenBy(x => x.Name).ToList();

        int? Resolve(int? configuredId, string defaultName, out bool missing)
        {
            if (configuredId > 0)
            {
                missing = statuses.All(x => x.Id != configuredId);
                return configuredId;
            }

            var byName = statuses.FirstOrDefault(x => x.ProjectId == null && string.Equals(x.Name, defaultName, StringComparison.OrdinalIgnoreCase))
                ?? statuses.FirstOrDefault(x => string.Equals(x.Name, defaultName, StringComparison.OrdinalIgnoreCase));
            missing = byName == null;
            return byName?.Id;
        }

        model.MergedStatusId = Resolve(settings.MergedStatusId, MergeStatusDefaults.Merged, out var mergedMissing);
        model.MergedStatusMissing = mergedMissing;
        model.RejectedStatusId = Resolve(settings.RejectedStatusId, MergeStatusDefaults.Rejected, out var rejectedMissing);
        model.RejectedStatusMissing = rejectedMissing;
        model.OpenedStatusId = Resolve(settings.OpenedStatusId, MergeStatusDefaults.Opened, out var openedMissing);
        model.OpenedStatusMissing = openedMissing;
        model.MoveTasksOnMergeRequestOpen = settings.MoveTasksOnMergeRequestOpen;

        foreach (var status in statuses)
            model.AvailableStatuses.Add(new SelectListItem
            {
                Text = status.ProjectId == null ? status.Name : $"{status.Name} ({status.Project?.Name})",
                Value = status.Id.ToString()
            });
    }

    private async Task PopulateWorkItemAsync(WorkItemSettingsModel model)
    {
        foreach (var user in await _userService.GetAllActiveAsync())
            model.AvailableUsers.Add(new SelectListItem
            {
                Text = user.Name,
                Value = user.Id.ToString()
            });
    }

    private static string ResolveActiveTab(ConfigurationModel model, string requested)
    {
        var tabs = new (string Key, bool Can)[]
        {
            ("notifications", model.CanNotifications),
            ("general", model.CanGeneral),
            ("security", model.CanSecurity),
            ("workitem", model.CanWorkItem),
            ("integration", model.CanIntegration),
            ("reports", model.CanReports),
            ("resource", model.CanResource),
            ("stageduration", model.CanStageDuration)
        };

        if (!string.IsNullOrWhiteSpace(requested) && tabs.Any(t => t.Key == requested && t.Can))
            return requested;

        return tabs.FirstOrDefault(t => t.Can).Key ?? "notifications";
    }
}
