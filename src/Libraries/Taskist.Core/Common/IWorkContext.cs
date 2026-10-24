using Taskist.Core.Domain.Localization;
using Taskist.Core.Domain.Users;

namespace Taskist.Core.Common;

public interface IWorkContext
{
    Task<User> GetCurrentUserAsync();

    int? GetCurrentUserId();

    int? GetCurrentUserLanguageId();

    Task<Language> GetCurrentUserLanguageAsync();
}
