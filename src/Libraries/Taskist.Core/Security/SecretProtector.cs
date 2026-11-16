using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Taskist.Core.Security;

public class SecretProtector : ISecretProtector
{
    #region Fields

    public const string Prefix = "dp1:";
    public const string DefaultPurpose = "Taskist.Secrets.v1";

    private readonly IDataProtectionProvider _provider;

    #endregion

    #region Ctor

    public SecretProtector(IDataProtectionProvider provider)
    {
        _provider = provider;
    }

    #endregion

    #region Methods

    public string? Protect(string? plainText, string? purpose = null)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        return Prefix + Protector(purpose).Protect(plainText);
    }

    public string? Unprotect(string? storedValue, string? purpose = null)
    {
        if (string.IsNullOrEmpty(storedValue))
            return storedValue;

        if (storedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            try
            {
                return Protector(purpose).Unprotect(storedValue[Prefix.Length..]);
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        return storedValue;
    }

    public bool IsCurrentFormat(string? storedValue)
    {
        return string.IsNullOrEmpty(storedValue) || storedValue.StartsWith(Prefix, StringComparison.Ordinal);
    }

    #endregion

    #region Utilities

    private IDataProtector Protector(string? purpose)
    {
        return _provider.CreateProtector(string.IsNullOrWhiteSpace(purpose) ? DefaultPurpose : purpose);
    }

    #endregion
}
