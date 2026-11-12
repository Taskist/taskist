using Microsoft.EntityFrameworkCore;
using Taskist.Core.Domain.Users;
using Taskist.Core.Security;
using Taskist.Data.Repository;

namespace Taskist.Service.Authentication;

public class TwoFactorService : ITwoFactorService
{
    #region Fields

    protected readonly IRepository<UserTwoFactor> _twoFactorRepository;
    protected readonly IRepository<UserRecoveryCode> _recoveryCodeRepository;

    #endregion

    #region Ctor

    public TwoFactorService(IRepository<UserTwoFactor> twoFactorRepository,
        IRepository<UserRecoveryCode> recoveryCodeRepository)
    {
        _twoFactorRepository = twoFactorRepository;
        _recoveryCodeRepository = recoveryCodeRepository;
    }

    #endregion

    #region Methods

    public async Task<UserTwoFactor> GetAsync(int userId)
    {
        return await _twoFactorRepository.Table.FirstOrDefaultAsync(x => x.UserId == userId);
    }

    public async Task<bool> IsEnabledAsync(int userId)
    {
        return await _twoFactorRepository.Table.AnyAsync(x => x.UserId == userId);
    }

    public async Task<IReadOnlyList<string>> EnableAsync(int userId, string secret, string code)
    {
        var step = Totp.Verify(secret, code, DateTimeOffset.UtcNow, 0);
        if (step == null)
            return null;

        var existing = await GetAsync(userId);
        if (existing != null)
            await _twoFactorRepository.DeleteAsync(existing);

        await _twoFactorRepository.InsertAsync(new UserTwoFactor
        {
            UserId = userId,
            Secret = secret,
            EnabledOn = DateTime.UtcNow,
            LastUsedTimeStep = step.Value
        });

        return await RegenerateRecoveryCodesAsync(userId);
    }

    public async Task<bool> VerifyCodeAsync(int userId, string code)
    {
        var entity = await GetAsync(userId);
        if (entity == null)
            return false;

        var step = Totp.Verify(entity.Secret, code, DateTimeOffset.UtcNow, entity.LastUsedTimeStep);
        if (step == null)
            return false;

        entity.LastUsedTimeStep = step.Value;
        await _twoFactorRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<bool> UseRecoveryCodeAsync(int userId, string code)
    {
        var hash = RecoveryCodes.Hash(code);
        if (hash == null)
            return false;

        var entity = await _recoveryCodeRepository.Table
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CodeHash == hash && x.UsedOn == null);
        if (entity == null)
            return false;

        entity.UsedOn = DateTime.UtcNow;
        await _recoveryCodeRepository.UpdateAsync(entity);
        return true;
    }

    public async Task<IReadOnlyList<string>> RegenerateRecoveryCodesAsync(int userId)
    {
        await _recoveryCodeRepository.DeleteAsync(x => x.UserId == userId);

        var codes = RecoveryCodes.Generate();
        var now = DateTime.UtcNow;
        await _recoveryCodeRepository.InsertAsync(codes
            .Select(c => new UserRecoveryCode { UserId = userId, CodeHash = RecoveryCodes.Hash(c)!, CreatedOn = now })
            .ToList());

        return codes;
    }

    public async Task<int> CountRemainingRecoveryCodesAsync(int userId)
    {
        return await _recoveryCodeRepository.Table.CountAsync(x => x.UserId == userId && x.UsedOn == null);
    }

    public async Task<bool> DisableAsync(int userId)
    {
        var entity = await GetAsync(userId);
        await _recoveryCodeRepository.DeleteAsync(x => x.UserId == userId);
        if (entity == null)
            return false;

        await _twoFactorRepository.DeleteAsync(entity);
        return true;
    }

    #endregion
}
