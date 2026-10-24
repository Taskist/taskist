using Taskist.Core.Domain.Users;

namespace Taskist.Service.Authentication;

public interface IAuthenticationService
{
    Task SignInAsync(User entity, bool isPersistent);

    Task SignOutAsync();

    Task<User> GetAuthenticatedUserAsync();
}