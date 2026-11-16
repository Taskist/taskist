using Taskist.Core.Domain.Users;

namespace Taskist.Service.Authentication;

public interface ITwoFactorService
{
    Task<UserTwoFactor> GetAsync(int userId);

    Task<bool> IsEnabledAsync(int userId);

    Task<IReadOnlyList<string>> EnableAsync(int userId, string secret, string code);

    Task<bool> VerifyCodeAsync(int userId, string code);

    Task<bool> UseRecoveryCodeAsync(int userId, string code);

    Task<IReadOnlyList<string>> RegenerateRecoveryCodesAsync(int userId);

    Task<int> CountRemainingRecoveryCodesAsync(int userId);

    Task<bool> DisableAsync(int userId);
}
