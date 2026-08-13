using Taskist.Core.Domain.Users;

namespace Taskist.Service.Security;

public interface IEncryptionService
{
    string CreateSaltKey(int size);

    /// <summary>
    /// Creates a salt sized for the current password format.
    /// </summary>
    string CreatePasswordSalt();

    string CreatePasswordHash(string password, string saltKey);

    string CreatePasswordHash(string password, string saltKey, PasswordFormat format);

    /// <summary>
    /// Verifies a password against a stored hash using a constant time comparison.
    /// </summary>
    bool VerifyPassword(string password, string saltKey, string expectedHash, PasswordFormat format);

    string EncryptText(string plainText, string encryptionPrivateKey = "");

    string DecryptText(string cipherText, string encryptionPrivateKey = "");

    string GenerateToken(Guid userCode);

    bool ValidateToken(string token, int expiryTimeInMinutes = 180);
}
