namespace Taskist.Core.Common;

/// <summary>Activity-log event type keys (ActivityLog.SystemName).</summary>
public static class ActivityType
{
    public const string Login = "Login";

    public const string LoginFailed = "LoginFailed";

    public const string Logout = "Logout";

    public const string AccessDenied = "AccessDenied";

    public const string Export = "Export";
}
