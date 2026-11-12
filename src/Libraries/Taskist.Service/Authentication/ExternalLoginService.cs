using Microsoft.EntityFrameworkCore;
using Taskist.Core.Domain.Users;
using Taskist.Data.Repository;

namespace Taskist.Service.Authentication;

public class ExternalLoginService : IExternalLoginService
{
    #region Fields

    protected readonly IRepository<UserExternalLogin> _externalLoginRepository;

    #endregion

    #region Ctor

    public ExternalLoginService(IRepository<UserExternalLogin> externalLoginRepository)
    {
        _externalLoginRepository = externalLoginRepository;
    }

    #endregion

    #region Methods

    public async Task<UserExternalLogin> GetAsync(string provider, string subject)
    {
        if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(subject))
            return null;

        return await _externalLoginRepository.Table
            .FirstOrDefaultAsync(x => x.Provider == provider && x.Subject == subject);
    }

    public async Task<IList<UserExternalLogin>> GetForUserAsync(int userId)
    {
        return await _externalLoginRepository.Table
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.CreatedOn)
            .ToListAsync();
    }

    public async Task<int> CountForUserAsync(int userId)
    {
        return await _externalLoginRepository.Table.CountAsync(x => x.UserId == userId);
    }

    public async Task InsertAsync(UserExternalLogin entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await _externalLoginRepository.InsertAsync(entity);
    }

    public async Task MarkUsedAsync(UserExternalLogin entity, string email)
    {
        ArgumentNullException.ThrowIfNull(entity);

        entity.LastUsedOn = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(email))
            entity.Email = email.Length > 150 ? email[..150] : email;

        await _externalLoginRepository.UpdateAsync(entity);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var entity = await _externalLoginRepository.Table.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (entity == null)
            return false;

        await _externalLoginRepository.DeleteAsync(entity);
        return true;
    }

    #endregion
}
