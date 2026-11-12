using Taskist.Core.Domain.Users;

namespace Taskist.Service.Authentication;

public interface IAuthenticationService
{
    Task SignInAsync(User entity, bool isPersistent);

    Task SignInAsync(User entity, bool isPersistent, string authMethod);

    Task SignOutAsync();

    Task<User> GetAuthenticatedUserAsync();
}