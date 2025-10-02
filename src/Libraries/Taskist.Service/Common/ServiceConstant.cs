namespace Taskist.Service.Common;

public static class ServiceConstant
{
    #region Permissions

    public static string PermissionsAllowedCacheKey => "taskist.per.allowed-{0}-{1}";

    public static string PermissionsAllByUserRoleIdCacheKey => "taskist.per.allbyuserroleid-{0}";

    public static string PermissionsPatternCacheKey => "taskist.per.";

    #endregion

    #region Auth

    public static string ClaimsIssuer => "taskist";

    #endregion Auth

    #region Caching

    public static string PermissionsPrefixCacheKey => "taskist.per.";

    public static string UserRolesPrefixCacheKey => "taskist.userrole.";

    public static string UserRolesBySystemNameCacheKey => "taskist.user.systemname-{0}";

    public static string UserRolesAllCacheKey => "taskist.userrole.all-{0}";

    public static string UserRoleIdsCacheKey => "taskist.userrole.ids.{0}-{1}";

    public static string MenuCacheKeyByUser => "taskist.menu-{0}";

    public static string MenuResourceCacheKey => "taskist.menures";

    public static string LangCacheKey => "taskist.lang";

    public static string ActivityTypesCacheKey => "taskist.acttype";

    public static string LocaleStringResourcesByNameCacheKey => "taskist.localestringresource.byname.{0}-{1}";

    public static string GenericAttributeCacheKey => "taskist.genericattribute.{0}-{1}";

    public static string SettingsAllAsDictionaryCacheKey => "taskist.settings.all.dictionary";

    public static string AccessibleProjectCacheKey => "wc.user.ids.projects";

    #endregion

    #region Defaults

    public static int GridDefaultPageSize = 10;

    public static int SaltKeySize = 10;

    #endregion

    #region Messages

    public static string NotificationListKey => "taskist.notificationList";

    public static string MessageTemplatesByNamePrefix => "taskist.messagetemplate.byname.{0}";

    #endregion
}
