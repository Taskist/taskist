using Taskist.Core.Common;
using System.Security.Cryptography;
using System.Text;

namespace Taskist.Service.Security;

public class EncryptionService : IEncryptionService
{
    #region Field

    protected const string EncryptionKey = "E546C8DF278CD5931069B522E695D4F2";

    #endregion

    #region Utilities

    protected static byte[] EncryptTextToMemory(string data, SymmetricAlgorithm provider)
    {
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, provider.CreateEncryptor(), CryptoStreamMode.Write))
        {
            var toEncrypt = Encoding.Unicode.GetBytes(data);
            cs.Write(toEncrypt, 0, toEncrypt.Length);
            cs.FlushFinalBlock();
        }

        return ms.ToArray();
    }

    protected static string DecryptTextFromMemory(byte[] data, SymmetricAlgorithm provider)
    {
        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, provider.CreateDecryptor(), CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.Unicode);

        return sr.ReadToEnd();
    }

    protected SymmetricAlgorithm GetEncryptionAlgorithm(string encryptionKey)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            throw new ArgumentNullException(nameof(encryptionKey));

        SymmetricAlgorithm provider = Aes.Create();
        var vectorBlockSize = provider.BlockSize / 8;

        provider.Key = Encoding.ASCII.GetBytes(encryptionKey[0..16]);
        provider.IV = Encoding.ASCII.GetBytes(encryptionKey[^vectorBlockSize..]);

        return provider;
    }

    #endregion

    #region Methods

    public string CreateSaltKey(int size)
    {
        //generate a cryptographic random number
        using var provider = RandomNumberGenerator.Create();
        var buff = new byte[size];
        provider.GetBytes(buff);

        // Return a Base64 string representation of the random number
        return Convert.ToBase64String(buff);
    }

    public string CreatePasswordHash(string password, string saltkey)
    {
        return HashHelper.CreateHash(Encoding.UTF8.GetBytes(string.Concat(password, saltkey)), "SHA1");
    }

    public string EncryptText(string plainText, string encryptionPrivateKey = "")
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        if (string.IsNullOrEmpty(encryptionPrivateKey))
            encryptionPrivateKey = EncryptionKey;

        using var provider = GetEncryptionAlgorithm(encryptionPrivateKey);
        var encryptedBinary = EncryptTextToMemory(plainText, provider);

        return Convert.ToBase64String(encryptedBinary);
    }

    public string DecryptText(string cipherText, string encryptionPrivateKey = "")
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        if (string.IsNullOrEmpty(encryptionPrivateKey))
            encryptionPrivateKey = EncryptionKey;

        using var provider = GetEncryptionAlgorithm(encryptionPrivateKey);

        var buffer = Convert.FromBase64String(cipherText);
        return DecryptTextFromMemory(buffer, provider);
    }

    #endregion

    #region Token

    public string GenerateToken(Guid userCode)
    {
        byte[] time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
        byte[] key = Guid.NewGuid().ToByteArray();
        string plainToken = $"{userCode}|{Convert.ToBase64String(time.Concat(key).ToArray())}";

        string token = EncryptText(plainToken);
        return token;
    }

    public bool ValidateToken(string token, int expiryTimeInMinutes = 180)
    {
        byte[] data = Convert.FromBase64String(token);
        DateTime createdDate = DateTime.FromBinary(BitConverter.ToInt64(data, 0));
        return createdDate > DateTime.UtcNow.AddMinutes(-expiryTimeInMinutes);
    }

    #endregion
}