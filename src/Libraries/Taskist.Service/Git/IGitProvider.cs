using Taskist.Core.Enums;

namespace Taskist.Service.Git;

public interface IGitProvider : IGitRepositoryApi, IGitPullRequestApi, IGitReviewApi, IGitUserApi, IGitIssueApi, IGitOAuthApi, IGitWebhookApi
{
    GitProviderType ProviderType { get; }

    GitProviderCapabilities Capabilities { get; }

    int ConnectionId { get; }

    bool IsConfigured { get; }

    string? LastError { get; }
}
