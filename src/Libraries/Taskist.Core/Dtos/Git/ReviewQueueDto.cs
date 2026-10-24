namespace Taskist.Core.Dtos.Git;

public class ReviewQueueDto
{
    public List<ReviewQueueItemDto> Items { get; set; } = [];

    public List<ReviewProjectScopeDto> Projects { get; set; } = [];

    public List<string> FailedProjects { get; set; } = [];

    public string? Error { get; set; }

    public int WaitingCount => Items.Count(x => x.Status != ReviewMergeStatus.Draft);

    public int ReadyCount => Items.Count(x => x.Status == ReviewMergeStatus.Mergeable);

    public int ConflictCount => Items.Count(x => x.Status == ReviewMergeStatus.Conflict || x.Status == ReviewMergeStatus.NeedsRebase);

    public int StaleCount => Items.Count(x => x.IsStale);
}
