using Taskist.Core.Domain.Users;

namespace Taskist.Service.Authentication;

public interface IExternalLoginService
{
    Task<UserExternalLogin> GetAsync(string provider, string subject);

    Task<IList<UserExternalLogin>> GetForUserAsync(int userId);

    Task<int> CountForUserAsync(int userId);

    Task InsertAsync(UserExternalLogin entity);

    Task MarkUsedAsync(UserExternalLogin entity, string email);

    Task<bool> DeleteAsync(int id, int userId);
}
