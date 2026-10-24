using Hangfire;
using Taskist.Core.Domain.Git;
using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitWebhookService
{
    Task<GitWebhookReceiveResult> ReceiveAsync(ProjectRepository repository, Func<string, string?> header, byte[] body, string? fallbackSecret = null);

    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ProcessAsync(int deliveryId);

    Task<int> PurgeAsync(int keepDays);
}
