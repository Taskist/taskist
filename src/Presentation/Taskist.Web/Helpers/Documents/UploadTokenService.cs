using System.Globalization;
using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Taskist.Web.Helpers.Documents;

public class UploadTokenService : IUploadTokenService
{
    #region Fields

    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(1);

    private readonly ITimeLimitedDataProtector _protector;

    #endregion

    #region Ctor

    public UploadTokenService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("Taskist.UploadTokens.v1").ToTimeLimitedDataProtector();
    }

    #endregion

    #region Methods

    public string Create(int userId, int documentId)
    {
        return _protector.Protect(string.Create(CultureInfo.InvariantCulture, $"{userId}:{documentId}"), Lifetime);
    }

    public int? Read(string? token, int userId)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        string payload;
        try
        {
            payload = _protector.Unprotect(token);
        }
        catch (CryptographicException)
        {
            return null;
        }

        var parts = payload.Split(':');
        if (parts.Length != 2
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var owner)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var documentId)
            || owner != userId || documentId <= 0)
            return null;

        return documentId;
    }

    #endregion
}
