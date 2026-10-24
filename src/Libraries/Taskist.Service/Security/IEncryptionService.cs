using Taskist.Core.Enums;

namespace Taskist.Service.Security;

public interface IEncryptionService
{
    string HashPassword(string password);

    PasswordCheckResult VerifyPassword(string password, string storedHash);

    string EncryptText(string plainText);

    string DecryptText(string cipherText);

    string GenerateToken(Guid userCode);

    bool ValidateToken(string token, int expiryTimeInMinutes = 180);
}
