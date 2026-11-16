using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitWebhookApi
{
    Task<GitResult<object>> RegisterWebhookAsync(string repoRef, GitWebhookRegistrationDto registration, string userToken);
}
