using Taskist.Core.Domain.Users;

namespace Taskist.Service.Security;

public class PermissionProvider : IPermissionProvider
{
    public static readonly UserRolePermission ManageDashboard = new() { Name = "Manage Dashboard", SystemName = "ManageDashboard", RoleGroup = "Dashboard", SystemDefined = true };
    public static readonly UserRolePermission ManageCompany = new() { Name = "Manage Company", SystemName = "ManageCompany", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageMaster = new() { Name = "Manage Master", SystemName = "ManageMaster", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageUser = new() { Name = "Manage User", SystemName = "ManageUser", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageUserRole = new() { Name = "Manage User Role", SystemName = "ManageUserRole", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageDepartment = new() { Name = "Manage Department", SystemName = "ManageDepartment", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageReporter = new() { Name = "Manage Reporter", SystemName = "ManageReporter", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageDesignation = new() { Name = "Manage Designation", SystemName = "ManageDesignation", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageCountry = new() { Name = "Manage Country", SystemName = "ManageCountry", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageStateProvince = new() { Name = "Manage State Province", SystemName = "ManageStateProvince", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageCurrency = new() { Name = "Manage Currency", SystemName = "ManageCurrency", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageLanguage = new() { Name = "Manage Language", SystemName = "ManageLanguage", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageLocaleResource = new() { Name = "Manage Locale Resource", SystemName = "ManageLocaleResource", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageEmailAccount = new() { Name = "Manage Email Account", SystemName = "ManageEmailAccount", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageEmailTemplate = new() { Name = "Manage Email Template", SystemName = "ManageEmailTemplate", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageSetting = new() { Name = "Manage Setting", SystemName = "ManageSetting", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageSeverity = new() { Name = "Manage Severity", SystemName = "ManageSeverity", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageStatus = new() { Name = "Manage Status", SystemName = "ManageStatus", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageTaskType = new() { Name = "Manage Task Type", SystemName = "ManageTaskType", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageModule = new() { Name = "Manage Module", SystemName = "ManageModule", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageSubModule = new() { Name = "Manage Sub Module Source", SystemName = "ManageSubModule", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageClient = new() { Name = "Manage Client", SystemName = "ManageClient", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageProject = new() { Name = "Manage Project", SystemName = "ManageProject", RoleGroup = "Master", SystemDefined = true };
    public static readonly UserRolePermission ManageSprints = new() { Name = "Manage Sprints", SystemName = "ManageSprints", RoleGroup = "WorkItems", SystemDefined = true };
    public static readonly UserRolePermission ManageBoard = new() { Name = "Manage Boards", SystemName = "ManageBoard", RoleGroup = "WorkItems", SystemDefined = true };
    public static readonly UserRolePermission ManageBacklog = new() { Name = "Manage Backlog", SystemName = "ManageBacklog", RoleGroup = "WorkItems", SystemDefined = true };

    public virtual IEnumerable<UserRolePermission> GetPermissions()
    {
        return
        [
            ManageDashboard,
            ManageMaster,
            ManageUser,
            ManageUserRole,
            ManageDepartment,
            ManageDesignation,
            ManageCountry,
            ManageStateProvince,
            ManageCurrency,
            ManageLanguage,
            ManageLocaleResource,
            ManageSetting
        ];
    }
}