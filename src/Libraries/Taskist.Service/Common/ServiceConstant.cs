namespace Taskist.Service.Common;

public static class ServiceConstant
{
    #region Permissions

    public static string PermissionsAllowedCacheKey => "wc.bl.per.allowed-{0}-{1}";

    public static string PermissionsAllByUserRoleIdCacheKey => "wc.bl.per.allbyuserroleid-{0}";

    public static string PermissionsPatternCacheKey => "wc.bl.per.";

    #endregion

    #region Auth

    public static string ClaimsIssuer => "wc.bl";

    public static string IsAdminClaim => "wc.bl.isadmin";

    public static string LanguageIdClaim => "wc.bl.languageid";

    #endregion Auth

    #region Caching

    public static string PermissionsPrefixCacheKey => "wc.bl.per.";

    public static string UserRolesPrefixCacheKey => "wc.bl.userrole.";

    public static string UserRolesBySystemNameCacheKey => "wc.bl.user.systemname-{0}";

    public static string UserRolesAllCacheKey => "wc.bl.userrole.all-{0}";

    public static string UserRoleIdsCacheKey => "wc.bl.userrole.ids.{0}-{1}";

    public static string MenuCacheKeyByUser => "wc.bl.menu-{0}";

    public static string MenuResourceCacheKey => "wc.bl.menures";

    public static string LangCacheKey => "wc.bl.lang";

    public static string ActivityTypesCacheKey => "wc.bl.acttype";

    public static string LocaleStringResourcesByNameCacheKey => "wc.bl.localestringresource.byname.{0}-{1}";

    public static string GenericAttributeCacheKey => "wc.bl.genericattribute.{0}-{1}";

    public static string SettingsAllAsDictionaryCacheKey => "wc.bl.settings.all.dictionary";

    public static string AccessibleProjectCacheKey => "wc.user.ids.projects.{0}.{1}";

    #endregion

    #region Defaults

    public static int GridDefaultPageSize = 10;

    public static int SaltKeySize = 10;

    #endregion

    #region Messages

    public static string NotificationListKey => "wc.bl.notificationList";

    public static string MessageTemplatesByNamePrefix => "wc.bl.messagetemplate.byname.{0}";

    #endregion
}
