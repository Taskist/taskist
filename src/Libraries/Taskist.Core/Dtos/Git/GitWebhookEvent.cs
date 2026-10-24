using Taskist.Core.Enums;

namespace Taskist.Core.Dtos.Git;

public class GitWebhookEvent
{
    public GitEventKind Kind { get; set; }

    public string? Action { get; set; }

    public string? RepositoryExternalId { get; set; }

    public string? RepositoryPath { get; set; }

    public GitPersonDto? Actor { get; set; }

    public GitPushInfo? Push { get; set; }

    public GitMergeRequestInfo? MergeRequest { get; set; }

    public GitNoteInfo? Note { get; set; }

    public GitIssueDto? Issue { get; set; }

    public GitIssueCommentDto? IssueComment { get; set; }
}
