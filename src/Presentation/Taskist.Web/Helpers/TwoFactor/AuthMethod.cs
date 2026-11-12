namespace Taskist.Web.Helpers.TwoFactor;

public static class AuthMethod
{
    #region Fields

    public const string Local = "pwd";

    public const string LocalWithCode = "pwd+otp";

    public const string LocalRememberedDevice = "pwd+device";

    public const string SingleSignOn = "sso";

    #endregion
}
