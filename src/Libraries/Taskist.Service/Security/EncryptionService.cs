using Microsoft.Extensions.Options;
using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;
using System.Security.Cryptography;
using System.Text;
using Taskist.Core.Common;

namespace Taskist.Service.Security;

public class EncryptionService : IEncryptionService
{
    #region Field

    /// <summary>
    /// Marks a payload written with a random per-message IV.
    /// </summary>
    protected const byte EnvelopeVersion = 1;

    protected const int Pbkdf2Iterations = 210_000;

    protected const int Pbkdf2SaltSize = 16;

    protected const int Pbkdf2HashSize = 32;

    protected readonly SecurityOptions _securityOptions;

    #endregion

    #region Ctor

    public EncryptionService(IOptions<SecurityOptions> securityOptions)
    {
        _securityOptions = securityOptions.Value;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Derives a stable 256-bit AES key from the configured secret.
    /// </summary>
    protected static byte[] DeriveKey(string encryptionKey)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(encryptionKey));
    }

    protected string ResolveKey(string encryptionPrivateKey)
    {
        return string.IsNullOrEmpty(encryptionPrivateKey)
            ? _securityOptions.EncryptionKey
            : encryptionPrivateKey;
    }

    #endregion

    #region Methods

    public string CreateSaltKey(int size)
    {
        //generate a cryptographic random number
        var buff = RandomNumberGenerator.GetBytes(size);

        // Return a Base64 string representation of the random number
        return Convert.ToBase64String(buff);
    }

    public string CreatePasswordHash(string password, string saltkey)
    {
        return CreatePasswordHash(password, saltkey, PasswordFormat.Pbkdf2);
    }

    public string CreatePasswordHash(string password, string saltkey, PasswordFormat format)
    {
        ArgumentNullException.ThrowIfNull(password);
        ArgumentNullException.ThrowIfNull(saltkey);

        if (format == PasswordFormat.Sha1Legacy)
        {
            //retained only to verify credentials created by earlier versions
            return HashHelper.CreateHash(Encoding.UTF8.GetBytes(string.Concat(password, saltkey)), "SHA1");
        }

        var salt = Convert.FromBase64String(saltkey);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256,
            Pbkdf2HashSize);

        return Convert.ToBase64String(hash);
    }

    public string CreatePasswordSalt()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(Pbkdf2SaltSize));
    }

    public bool VerifyPassword(string password, string saltKey, string expectedHash, PasswordFormat format)
    {
        if (password == null || saltKey == null || expectedHash == null)
            return false;

        try
        {
            var actual = CreatePasswordHash(password, saltKey, format);

            //compare in constant time so the response cannot be used as an oracle
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(actual),
                Encoding.UTF8.GetBytes(expectedHash));
        }
        catch (FormatException)
        {
            //stored salt is not valid base64 - treat as a failed verification
            return false;
        }
    }

    public string EncryptText(string plainText, string encryptionPrivateKey = "")
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var key = DeriveKey(ResolveKey(encryptionPrivateKey));

        using var provider = Aes.Create();
        provider.Key = key;

        //a fresh IV per message; a fixed IV leaks equality between ciphertexts
        provider.GenerateIV();

        using var ms = new MemoryStream();

        ms.WriteByte(EnvelopeVersion);
        ms.Write(provider.IV, 0, provider.IV.Length);

        using (var cs = new CryptoStream(ms, provider.CreateEncryptor(), CryptoStreamMode.Write))
        {
            var toEncrypt = Encoding.UTF8.GetBytes(plainText);
            cs.Write(toEncrypt, 0, toEncrypt.Length);
            cs.FlushFinalBlock();
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string DecryptText(string cipherText, string encryptionPrivateKey = "")
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var buffer = Convert.FromBase64String(cipherText);

        using var provider = Aes.Create();
        provider.Key = DeriveKey(ResolveKey(encryptionPrivateKey));

        var ivLength = provider.BlockSize / 8;

        if (buffer.Length < 1 + ivLength || buffer[0] != EnvelopeVersion)
            throw new CryptographicException("The payload is not in a recognised format.");

        provider.IV = buffer[1..(1 + ivLength)];

        using var ms = new MemoryStream(buffer, 1 + ivLength, buffer.Length - 1 - ivLength);
        using var cs = new CryptoStream(ms, provider.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);

        return sr.ReadToEnd();
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
        if (string.IsNullOrEmpty(token))
            return false;

        try
        {
            byte[] data = Convert.FromBase64String(token);

            if (data.Length < sizeof(long))
                return false;

            DateTime createdDate = DateTime.FromBinary(BitConverter.ToInt64(data, 0));

            //a token stamped in the future is malformed or tampered with
            if (createdDate > DateTime.UtcNow.AddMinutes(1))
                return false;

            return createdDate > DateTime.UtcNow.AddMinutes(-expiryTimeInMinutes);
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            return false;
        }
    }

    #endregion
}
