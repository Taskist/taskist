namespace Taskist.Core.Security;

public static class OtpAuthUri
{
    #region Methods

    public static string Build(string issuer, string account, string base32Secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
        ArgumentException.ThrowIfNullOrWhiteSpace(account);
        ArgumentException.ThrowIfNullOrWhiteSpace(base32Secret);

        var label = $"{Uri.EscapeDataString(issuer.Trim())}:{Uri.EscapeDataString(account.Trim())}";

        return $"otpauth://totp/{label}?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer.Trim())}"
            + $"&algorithm=SHA1&digits={Totp.Digits}&period={Totp.PeriodSeconds}";
    }

    #endregion
}
