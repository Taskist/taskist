namespace Taskist.Core.Dtos.Git;

public class MergeRequestContextDto
{
    public string? Error { get; set; }

    public bool NeedsConnection { get; set; }

    public string? ConnectionMessage { get; set; }

    public string? SourceBranch { get; set; }

    public string? DefaultTarget { get; set; }

    public List<string> TargetBranches { get; set; } = [];

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? ReviewerName { get; set; }

    public List<int> TaskIds { get; set; } = [];

    public int? ExistingIid { get; set; }

    public string? ExistingUrl { get; set; }

    public string? OpenedStatusName { get; set; }

    public string ShortTerm { get; set; } = "MR";

    public string Term { get; set; } = "merge request";

    public string RefPrefix { get; set; } = "!";

    public string? RepositoryName { get; set; }
}
