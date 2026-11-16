using Taskist.Core.Dtos.Git;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public interface ICodeReviewWebhookHandler
{
    Task HandleMergeRequestAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent);

    Task HandleNoteAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent);

    Task<string?> AuthorUserNameAsync(GitRepositoryContext repository, GitMergeRequestInfo mergeRequest);
}
