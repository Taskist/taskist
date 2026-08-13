namespace Taskist.Web.Helpers.Common;

public static class WebConstant
{
    #region Defaults Settings

    public static string UserCookie => "taskist.user";

    public static string CultureCookie => "taskist.culture";

    public static int UserCookieExpires => 2;

    public static string AvatarFolderDefault => "images";

    public static string AvatarFolder => "uploads/avatars";

    public static long MaxAvatarBytes => 2 * 1024 * 1024;

    #endregion

    #region Rate limiting

    /// <summary>
    /// Declared as a const because attribute arguments must be compile time constants.
    /// </summary>
    public const string AuthRateLimitPolicy = "auth";

    #endregion
}
