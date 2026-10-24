using Taskist.Core.Dtos.Git;
using Taskist.Service.Git;

namespace Taskist.Service.WorkItems;

public class CodeReviewWebhookHandler : ICodeReviewWebhookHandler
{
    #region Fields

    private static readonly string[] TaskistNotePrefixes = ["**Rejected** in Taskist", "**Changes requested** in Taskist"];

    protected readonly ICodeReviewNotificationService _codeReviewNotificationService;

    #endregion

    #region Ctor

    public CodeReviewWebhookHandler(ICodeReviewNotificationService codeReviewNotificationService)
    {
        _codeReviewNotificationService = codeReviewNotificationService;
    }

    #endregion

    #region Methods

    public async Task HandleMergeRequestAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent)
    {
        var mr = webhookEvent.MergeRequest;
        if (mr == null || mr.Iid <= 0)
            return;

        var action = mr.Action;
        var actorUserName = webhookEvent.Actor?.UserName;
        var actorName = webhookEvent.Actor?.Name;

        if (action is not ("approved" or "close") && mr.NewReviewerUserNames.Count == 0)
            return;

        var mergeRequest = new MergeRequestRefDto
        {
            Iid = mr.Iid,
            Title = mr.Title,
            WebUrl = mr.Url,
            AuthorUserName = await AuthorUserNameAsync(repository, mr),
            Sha = mr.Sha,
            ConnectionId = repository.Connection.Id,
            RepositoryId = repository.RepositoryId,
            RefPrefix = repository.Capabilities.RefPrefix
        };

        var projectId = repository.Repository.ProjectId;
        if (mr.NewReviewerUserNames.Count > 0)
            await _codeReviewNotificationService.ReviewerAssignedAsync(projectId, mergeRequest, mr.NewReviewerUserNames, actorUserName);

        if (action == "approved")
            await _codeReviewNotificationService.ApprovedAsync(projectId, mergeRequest, actorUserName, actorName ?? actorUserName ?? "a reviewer");
        else if (action == "close")
            await _codeReviewNotificationService.ClosedAsync(projectId, mergeRequest, actorUserName, actorName);
    }

    public async Task HandleNoteAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent)
    {
        var note = webhookEvent.Note;
        if (note == null || note.System || note.MergeRequestIid <= 0)
            return;

        var body = note.Body;
        if (string.IsNullOrWhiteSpace(body) || TaskistNotePrefixes.Any(p => body.TrimStart().StartsWith(p, StringComparison.Ordinal)))
            return;

        var mergeRequest = new MergeRequestRefDto
        {
            Iid = note.MergeRequestIid,
            Title = note.MergeRequestTitle,
            WebUrl = note.MergeRequestUrl,
            AuthorUserName = note.MergeRequestAuthorUserName
                             ?? (webhookEvent.MergeRequest == null ? null : await AuthorUserNameAsync(repository, webhookEvent.MergeRequest)),
            ConnectionId = repository.Connection.Id,
            RepositoryId = repository.RepositoryId,
            RefPrefix = repository.Capabilities.RefPrefix
        };

        await _codeReviewNotificationService.CommentedAsync(repository.Repository.ProjectId, mergeRequest,
            webhookEvent.Actor?.UserName, webhookEvent.Actor?.Name, body, note.Url);
    }

    public async Task<string?> AuthorUserNameAsync(GitRepositoryContext repository, GitMergeRequestInfo mergeRequest)
    {
        if (!string.IsNullOrWhiteSpace(mergeRequest.AuthorUserName))
            return mergeRequest.AuthorUserName;

        if (mergeRequest.AuthorExternalId <= 0)
            return null;

        try
        {
            return (await repository.Provider.GetUserByIdAsync(mergeRequest.AuthorExternalId))?.UserName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    #endregion
}
