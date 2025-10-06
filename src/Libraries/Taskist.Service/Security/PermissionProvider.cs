namespace Taskist.Service.Security;

public class PermissionProvider
{
    public partial class General
    {
        public const string MANAGE_DASHBOARD = $"{nameof(General)}.ManageDashboard";
    }

    public partial class Configuration
    {
        public const string MANAGE_USER = $"{nameof(Configuration)}.ManageUser";
        public const string MANAGE_USER_ROLE = $"{nameof(Configuration)}.ManageUserRole";
        public const string MANAGE_REPORTER = $"{nameof(Configuration)}.ManageReporter";
        public const string MANAGE_LANGUAGE = $"{nameof(Configuration)}.ManageLanguage";
        public const string MANAGE_SETTINGS = $"{nameof(Configuration)}.ManageSetting";
        public const string MANAGE_SEVERITY = $"{nameof(Configuration)}.ManageSeverity";
        public const string MANAGE_STATUS = $"{nameof(Configuration)}.ManageStatus";
        public const string MANAGE_TASK_TYPE = $"{nameof(Configuration)}.ManageTaskType";
        public const string MANAGE_MODULE = $"{nameof(Configuration)}.ManageModule";
        public const string MANAGE_SUB_MODULE = $"{nameof(Configuration)}.ManageSubModule";
        public const string MANAGE_CLIENT = $"{nameof(Configuration)}.ManageClient";
        public const string MANAGE_PROJECT = $"{nameof(Configuration)}.ManageProject";
        public const string MANAGE_EMAIL_ACCOUNTS = $"{nameof(Configuration)}.ManageEmailAccount";
        public const string MANAGE_EMAIL_TEMPLATE = $"{nameof(Configuration)}.ManageEmailTemplate";
    }

    public partial class WorkItem
    {
        public const string MANAGE_BACKLOGLOG = $"{nameof(WorkItem)}.ManageBacklog";
        public const string MANAGE_BOARD = $"{nameof(WorkItem)}.ManageBoard";
        public const string MANAGE_SPRINT = $"{nameof(WorkItem)}.ManageSprints";
    }
}