using Taskist.Core.Domain.Users;

namespace Taskist.Service.Security;

public class PermissionProvider : IPermissionProvider
{
    public static readonly UserRolePermission ManageDashboard = new() { Name = "Manage Dashboard", SystemName = "ManageDashboard", RoleGroup = "Dashboard", SystemDefined = true };
    public static readonly UserRolePermission ViewExecutiveDashboard = new() { Name = "View Executive Dashboard", SystemName = "ViewExecutiveDashboard", RoleGroup = "Dashboard", SystemDefined = true };
    public static readonly UserRolePermission ManageUser = new() { Name = "Manage User", SystemName = "ManageUser", RoleGroup = "Organization", SystemDefined = true };
    public static readonly UserRolePermission ManageUserRole = new() { Name = "Manage User Role", SystemName = "ManageUserRole", RoleGroup = "Organization", SystemDefined = true };


    public static readonly UserRolePermission ManageConfiguration = new() { Name = "Manage Configuration", SystemName = "ManageConfiguration", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManagePortfolio = new() { Name = "Portfolio", SystemName = "ManagePortfolio", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageWorkItemSetup = new() { Name = "Work Item Setup", SystemName = "ManageWorkItemSetup", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageIntegrations = new() { Name = "Integrations", SystemName = "ManageIntegrations", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageNotificationSettings = new() { Name = "Manage Notification Settings", SystemName = "ManageNotificationSettings", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageGeneralSettings = new() { Name = "Manage General Settings", SystemName = "ManageGeneralSettings", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageSecuritySettings = new() { Name = "Manage Security Settings", SystemName = "ManageSecuritySettings", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageWorkItemSettings = new() { Name = "Manage Work Item Settings", SystemName = "ManageWorkItemSettings", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageIntegrationSettings = new() { Name = "Manage Integration Settings", SystemName = "ManageIntegrationSettings", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageGitConnections = new() { Name = "Manage Git Connections", SystemName = "ManageGitConnections", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageNotificationChannels = new() { Name = "Manage Notification Channels", SystemName = "ManageNotificationChannels", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageReportSettings = new() { Name = "Manage Report Settings", SystemName = "ManageReportSettings", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageReporter = new() { Name = "Manage Reporter", SystemName = "ManageReporter", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ReviewMergeRequests = new() { Name = "Review Merge Requests", SystemName = "ReviewMergeRequests", RoleGroup = "Work Items", SystemDefined = true };
    public static readonly UserRolePermission MergeMergeRequests = new() { Name = "Merge Merge Requests", SystemName = "MergeMergeRequests", RoleGroup = "Work Items", SystemDefined = true };
    public static readonly UserRolePermission ManageLanguage = new() { Name = "Manage Language", SystemName = "ManageLanguage", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageLocaleResource = new() { Name = "Manage Locale Resource", SystemName = "ManageLocaleResource", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageEmailAccount = new() { Name = "Manage Email Account", SystemName = "ManageEmailAccount", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageEmailTemplate = new() { Name = "Manage Email Template", SystemName = "ManageEmailTemplate", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageSetting = new() { Name = "Manage Setting", SystemName = "ManageSetting", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageSeverity = new() { Name = "Manage Severity", SystemName = "ManageSeverity", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageStatus = new() { Name = "Manage Status", SystemName = "ManageStatus", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageTaskType = new() { Name = "Manage Task Type", SystemName = "ManageTaskType", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageTags = new() { Name = "Manage Tags", SystemName = "ManageTags", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageAnnouncement = new() { Name = "Manage Announcement", SystemName = "ManageAnnouncement", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageModule = new() { Name = "Manage Module", SystemName = "ManageModule", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageSubModule = new() { Name = "Manage Sub Module Source", SystemName = "ManageSubModule", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageClient = new() { Name = "Manage Client", SystemName = "ManageClient", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageProject = new() { Name = "Manage Project", SystemName = "ManageProject", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ManageJobs = new() { Name = "Manage Background Jobs", SystemName = "ManageJobs", RoleGroup = "Configuration", SystemDefined = true };
    public static readonly UserRolePermission ViewAuditLog = new() { Name = "View Audit Log", SystemName = "ViewAuditLog", RoleGroup = "Configuration", SystemDefined = true };

    public static readonly UserRolePermission ManageRelease = new() { Name = "Manage Release", SystemName = "ManageRelease", RoleGroup = "WorkItems", SystemDefined = true };
    public static readonly UserRolePermission ManageBoard = new() { Name = "Manage Boards", SystemName = "ManageBoard", RoleGroup = "WorkItems", SystemDefined = true };
    public static readonly UserRolePermission ManageBacklog = new() { Name = "Manage Backlog", SystemName = "ManageBacklog", RoleGroup = "WorkItems", SystemDefined = true };

    public static readonly UserRolePermission ManagePivotReport = new() { Name = "Manage Pivot Report", SystemName = "ManagePivotReport", RoleGroup = "Reports", SystemDefined = true };
    public static readonly UserRolePermission ManageAnalyticsReport = new() { Name = "Manage Analytics Report", SystemName = "ManageAnalyticsReport", RoleGroup = "Reports", SystemDefined = true };
    public static readonly UserRolePermission ManageResourceReport = new() { Name = "Manage Resource Report", SystemName = "ManageResourceReport", RoleGroup = "Reports", SystemDefined = true };
    public static readonly UserRolePermission ManageStageDuration = new() { Name = "Manage Stage Duration", SystemName = "ManageStageDuration", RoleGroup = "Reports", SystemDefined = true };
    public static readonly UserRolePermission ManageDocs = new() { Name = "Manage Docs", SystemName = "ManageDocs", RoleGroup = "Docs", SystemDefined = true };
    public static readonly UserRolePermission ManageWiki = new() { Name = "Manage Wiki", SystemName = "ManageWiki", RoleGroup = "Docs", SystemDefined = true };

    public virtual IEnumerable<UserRolePermission> GetPermissions()
    {
        return
        [
            ManageDashboard,
            ManageConfiguration,
            ManageNotificationSettings,
            ManageGeneralSettings,
            ManageSecuritySettings,
            ManageWorkItemSettings,
            ManageIntegrationSettings,
            ManageReportSettings,
            ManageUser,
            ManageUserRole,
            ManageLanguage,
            ManageLocaleResource,
            ManageSetting
        ];
    }
}