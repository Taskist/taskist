namespace Taskist.Web.Helpers.TwoFactor;

public static class AuthMethod
{
    #region Fields

    public const string Password = "pwd";

    public const string PasswordAndCode = "pwd+otp";

    public const string RememberedDevice = "pwd+device";

    public const string SingleSignOn = "sso";

    #endregion
}
