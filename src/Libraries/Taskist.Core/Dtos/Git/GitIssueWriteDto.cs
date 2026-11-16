namespace Taskist.Core.Dtos.Git;

public class GitIssueWriteDto
{
    public string? Title { get; set; }

    public string? Body { get; set; }

    public bool? Closed { get; set; }

    public List<string>? Labels { get; set; }

    public List<GitPersonDto>? Assignees { get; set; }
}
