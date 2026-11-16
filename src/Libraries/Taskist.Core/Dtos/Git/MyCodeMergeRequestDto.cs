namespace Taskist.Core.Dtos.Git;

public class MyCodeMergeRequestDto
{
    public int Iid { get; set; }

    public string RefPrefix { get; set; } = "!";

    public string? Title { get; set; }

    public string? ProjectPath { get; set; }

    public string? WebUrl { get; set; }

    public string? SourceBranch { get; set; }

    public string? TargetBranch { get; set; }

    public string Status { get; set; } = ReviewMergeStatus.Other;

    public string? AuthorName { get; set; }

    public List<string> ReviewerNames { get; set; } = [];

    public int NotesCount { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<int> TaskIds { get; set; } = [];
}
