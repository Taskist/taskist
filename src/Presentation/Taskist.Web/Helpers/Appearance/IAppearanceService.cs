using Taskist.Core.Domain.Users;

namespace Taskist.Web.Helpers.Appearance;

public interface IAppearanceService
{
    Task<AppearanceSettings> ResolveAsync();

    Task<AppearanceSettings> GetForUserAsync(User user);

    Task<AppearanceSettings> SaveAsync(User user, string? mode, string? accent);

    Task SyncCookieAsync(User user);
}
