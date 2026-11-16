using Taskist.Core.Enums;
using Taskist.Core.Security;
using System.Security.Cryptography;
using System.Text;

namespace Taskist.Service.Security;

public class EncryptionService : IEncryptionService
{
    #region Fields

    private const string TokenPurpose = "Taskist.Tokens.v1";
    private const string PasswordScheme = "pbkdf2-sha512";
    private const int PasswordIterations = 210000;
    private const int PasswordSaltBytes = 16;
    private const int PasswordHashBytes = 64;

    private readonly ISecretProtector _secretProtector;

    #endregion

    #region Ctor

    public EncryptionService(ISecretProtector secretProtector)
    {
        _secretProtector = secretProtector;
    }

    #endregion

    #region Methods

    public string HashPassword(string password)
    {
        ArgumentNullException.ThrowIfNull(password);

        var salt = RandomNumberGenerator.GetBytes(PasswordSaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, PasswordIterations, HashAlgorithmName.SHA512, PasswordHashBytes);

        return $"{PasswordScheme}${PasswordIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public PasswordCheckResult VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash)
            || !storedHash.StartsWith(PasswordScheme + "$", StringComparison.Ordinal))
            return PasswordCheckResult.Failed;

        var parts = storedHash.Split('$');
        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations) || iterations < 10000)
            return PasswordCheckResult.Failed;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return PasswordCheckResult.Failed;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA512, expected.Length);
        if (!CryptographicOperations.FixedTimeEquals(actual, expected))
            return PasswordCheckResult.Failed;

        return iterations < PasswordIterations ? PasswordCheckResult.SuccessRehashNeeded : PasswordCheckResult.Success;
    }

    public string EncryptText(string plainText)
    {
        return _secretProtector.Protect(plainText, TokenPurpose);
    }

    public string DecryptText(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText) || !cipherText.StartsWith(SecretProtector.Prefix, StringComparison.Ordinal))
            return null;

        return _secretProtector.Unprotect(cipherText, TokenPurpose);
    }

    #endregion

    #region Token

    public string GenerateToken(Guid userCode)
    {
        byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
        byte[] key = Guid.NewGuid().ToByteArray();
        string plainToken = $"{userCode}|{Convert.ToBase64String(time.Concat(key).ToArray())}";

        return EncryptText(plainToken);
    }

    public bool ValidateToken(string token, int expiryTimeInMinutes = 180)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var data = new byte[token.Length];
        if (!Convert.TryFromBase64String(token, data, out var length) || length < sizeof(long))
            return false;

        DateTime createdDate;
        try
        {
            createdDate = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
        }
        catch (ArgumentException)
        {
            return false;
        }

        return createdDate > DateTime.UtcNow.AddMinutes(-expiryTimeInMinutes);
    }

    #endregion
}
