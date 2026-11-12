using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Taskist.Web.Helpers.TwoFactor;

public static class RememberedDeviceToken
{
    #region Fields

    public const string Purpose = "Taskist.TwoFactor.RememberDevice.v1";

    #endregion

    #region Methods

    public static string Create(IDataProtector protector, int userId, long stamp, DateTimeOffset expiresUtc)
    {
        ArgumentNullException.ThrowIfNull(protector);

        var payload = string.Join('|',
            userId.ToString(CultureInfo.InvariantCulture),
            stamp.ToString(CultureInfo.InvariantCulture),
            expiresUtc.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

        return protector.Protect(payload);
    }

    public static bool IsValid(IDataProtector protector, string? token, int userId, long stamp, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(protector);

        if (string.IsNullOrWhiteSpace(token))
            return false;

        string payload;
        try
        {
            payload = protector.Unprotect(token);
        }
        catch (CryptographicException)
        {
            return false;
        }

        var parts = payload.Split('|');
        return parts.Length == 3
            && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var tokenUser) && tokenUser == userId
            && long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var tokenStamp) && tokenStamp == stamp
            && long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var expires)
            && DateTimeOffset.FromUnixTimeSeconds(expires) > nowUtc;
    }

    #endregion
}
