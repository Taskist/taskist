namespace Taskist.Core.Common;

public static class Constant
{
    #region App

    public static string Prefix => ".Taskist";

    public static string IsPostBeingDoneRequestItem => "Taskist.IsPOSTBeingDone";

    public static string SessionCookie => ".Session";

    public static string AntiForgeryCookie => ".Antiforgery";

    #endregion

    #region System user roles

    public static string AdministratorRoleName => "SystemAdministrator";

    public static string RegisteredRoleName => "Registered";

    public static string DeveloperRoleName => "Developer";

    public static string RepoterRoleName => "Reporter";

    public static string QualityAssuranceRoleName => "QualityAssurance";

    public static string LastVisitedPageAttribute => "LastVisitedPage";

    #endregion

    #region Settings

    public static string AllowedIPAddress => "AllowedIpAddress";

    #endregion

    #region Caching

    public static int CacheTime => 60;

    #endregion

    #region Session

    public static string BacklogFilterPresets => "BacklogFilterPresets";

    public static string BacklogVisibleColumns => "BacklogVisibleColumns";

    public static string BacklogGroupByParent => "BacklogGroupByParent";

    #endregion
}
