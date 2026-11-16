namespace Taskist.Core.Dtos.Git;

public class ReviewQueueItemDto
{
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public int RepositoryId { get; set; }

    public string RefPrefix { get; set; } = "!";

    public int Iid { get; set; }

    public string? Title { get; set; }

    public string? AuthorName { get; set; }

    public string? AuthorUserName { get; set; }

    public List<string> ReviewerUserNames { get; set; } = [];

    public List<string> ReviewerNames { get; set; } = [];

    public List<string> AssigneeUserNames { get; set; } = [];

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Status { get; set; } = ReviewMergeStatus.Other;

    public string? SourceBranch { get; set; }

    public string? TargetBranch { get; set; }

    public string? WebUrl { get; set; }

    public int NotesCount { get; set; }

    public List<int> TaskIds { get; set; } = [];

    public List<ReviewLinkedTaskDto> Tasks { get; set; } = [];

    public bool TitleIsTaskReference { get; set; }

    public bool IsMine { get; set; }

    public string? Sha { get; set; }

    public bool IsOwn { get; set; }

    public bool IsLeadProject { get; set; }

    public bool Unassigned => ReviewerUserNames.Count == 0;

    public bool IsStale => UpdatedAt < DateTime.UtcNow.AddDays(-3);
}
