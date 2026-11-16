using Hangfire;
using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git.Issues;

public interface IIssueSyncService
{
    Task HandleWebhookAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent);

    [AutomaticRetry(Attempts = 2, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    Task PushAsync(int backlogId);

    [AutomaticRetry(Attempts = 0)]
    [DisableConcurrentExecution(timeoutInSeconds: 600)]
    Task RunScheduledAsync();

    Task<TaskIssueDto> GetTaskIssueAsync(int backlogId, bool canManage);

    Task<bool> HasActiveLinkAsync(int backlogId);

    Task<IssueSyncResult> PublishAsync(int backlogId, int repositoryId, User user);

    Task<IssueSyncResult> SyncNowAsync(int backlogId, User user);

    Task<IssueSyncResult> UnlinkAsync(int backlogId, int linkId, User user);
}
