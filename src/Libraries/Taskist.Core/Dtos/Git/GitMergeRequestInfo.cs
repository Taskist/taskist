namespace Taskist.Core.Dtos.Git;

public class GitMergeRequestInfo
{
    public int Iid { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? State { get; set; }

    public string? Action { get; set; }

    public bool Draft { get; set; }

    public bool BecameReady { get; set; }

    public string? SourceBranch { get; set; }

    public string? TargetBranch { get; set; }

    public string? Url { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? UpdatedAt { get; set; }

    public long AuthorExternalId { get; set; }

    public string? AuthorUserName { get; set; }

    public string? Sha { get; set; }

    public List<string> NewReviewerUserNames { get; set; } = [];

    public bool IsReadyForReview => !Draft && (Action is "open" or "reopen" || BecameReady);
}
